using Microsoft.EntityFrameworkCore;
using PTJ.Application.Common;
using PTJ.Application.DTOs.Chat;
using PTJ.Application.Services;
using PTJ.Domain.Entities;
using PTJ.Domain.Interfaces;

namespace PTJ.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ChatConversationDto>> GetOrCreateConversationAsync(int userId, int recipientId, int? jobPostId = null, CancellationToken cancellationToken = default)
    {
        // Determine who is employer and who is student
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        var recipient = await _unitOfWork.Users.GetByIdAsync(recipientId, cancellationToken);

        if (user == null || recipient == null)
        {
            return Result<ChatConversationDto>.FailureResult("User not found");
        }

        var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == userId, cancellationToken);
        var recipientRoles = await _unitOfWork.UserRoles.FindAsync(ur => ur.UserId == recipientId, cancellationToken);

        var employerRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.Name == "EMPLOYER", cancellationToken);
        var studentRole = await _unitOfWork.Roles.FirstOrDefaultAsync(r => r.Name == "STUDENT", cancellationToken);

        bool isUserEmployer = userRoles.Any(ur => ur.RoleId == employerRole?.Id);
        bool isRecipientEmployer = recipientRoles.Any(ur => ur.RoleId == employerRole?.Id);

        int employerId = isUserEmployer ? userId : recipientId;
        int studentId = isUserEmployer ? recipientId : userId;

        // Check if conversation already exists
        var existingConversation = await _unitOfWork.ChatConversations.FirstOrDefaultAsync(
            c => c.EmployerId == employerId && c.StudentId == studentId,
            cancellationToken);

        if (existingConversation != null)
        {
            return Result<ChatConversationDto>.SuccessResult(await MapToDto(existingConversation, userId, cancellationToken));
        }

        // Create new conversation
        var conversation = new ChatConversation
        {
            EmployerId = employerId,
            StudentId = studentId,
            JobPostId = jobPostId
        };

        await _unitOfWork.ChatConversations.AddAsync(conversation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChatConversationDto>.SuccessResult(await MapToDto(conversation, userId, cancellationToken));
    }

    public async Task<Result<PaginatedList<ChatConversationDto>>> GetUserConversationsAsync(int userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var conversations = await _unitOfWork.ChatConversations.FindAsync(
            c => c.EmployerId == userId || c.StudentId == userId,
            cancellationToken);

        var orderedConversations = conversations
            .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = new List<ChatConversationDto>();
        foreach (var conversation in orderedConversations)
        {
            dtos.Add(await MapToDto(conversation, userId, cancellationToken));
        }

        var totalCount = conversations.Count();
        var result = new PaginatedList<ChatConversationDto>(dtos, totalCount, pageNumber, pageSize);

        return Result<PaginatedList<ChatConversationDto>>.SuccessResult(result);
    }

    public async Task<Result<PaginatedList<ChatMessageDto>>> GetConversationMessagesAsync(int conversationId, int userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null)
        {
            return Result<PaginatedList<ChatMessageDto>>.FailureResult("Conversation not found");
        }

        if (conversation.EmployerId != userId && conversation.StudentId != userId)
        {
            return Result<PaginatedList<ChatMessageDto>>.FailureResult("Unauthorized");
        }

        var messages = await _unitOfWork.ChatMessages.FindAsync(
            m => m.ConversationId == conversationId,
            cancellationToken);

        var orderedMessages = messages
            .OrderByDescending(m => m.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .OrderBy(m => m.CreatedAt)
            .ToList();

        var dtos = new List<ChatMessageDto>();
        foreach (var message in orderedMessages)
        {
            var sender = await _unitOfWork.Users.GetByIdAsync(message.SenderId, cancellationToken);
            dtos.Add(new ChatMessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = sender?.FullName ?? sender?.Email ?? "Unknown",
                Content = message.Content,
                IsRead = message.IsRead,
                ReadAt = message.ReadAt,
                CreatedAt = message.CreatedAt
            });
        }

        var totalCount = messages.Count();
        var result = new PaginatedList<ChatMessageDto>(dtos, totalCount, pageNumber, pageSize);

        return Result<PaginatedList<ChatMessageDto>>.SuccessResult(result);
    }

    public async Task<Result<ChatMessageDto>> SendMessageAsync(int userId, SendMessageDto dto, CancellationToken cancellationToken = default)
    {
        ChatConversation? conversation;

        if (dto.ConversationId.HasValue)
        {
            conversation = await _unitOfWork.ChatConversations.GetByIdAsync(dto.ConversationId.Value, cancellationToken);

            if (conversation == null)
            {
                return Result<ChatMessageDto>.FailureResult("Conversation not found");
            }

            if (conversation.EmployerId != userId && conversation.StudentId != userId)
            {
                return Result<ChatMessageDto>.FailureResult("Unauthorized");
            }
        }
        else if (dto.RecipientId.HasValue)
        {
            var createResult = await GetOrCreateConversationAsync(userId, dto.RecipientId.Value, dto.JobPostId, cancellationToken);
            if (!createResult.Success || createResult.Data == null)
            {
                return Result<ChatMessageDto>.FailureResult("Failed to create conversation");
            }

            conversation = await _unitOfWork.ChatConversations.GetByIdAsync(createResult.Data.Id, cancellationToken);
        }
        else
        {
            return Result<ChatMessageDto>.FailureResult("ConversationId or RecipientId is required");
        }

        if (conversation == null)
        {
            return Result<ChatMessageDto>.FailureResult("Conversation not found");
        }

        var message = new ChatMessage
        {
            ConversationId = conversation.Id,
            SenderId = userId,
            Content = dto.Content,
            IsRead = false
        };

        await _unitOfWork.ChatMessages.AddAsync(message, cancellationToken);

        conversation.LastMessageAt = DateTime.UtcNow;
        conversation.LastMessage = dto.Content.Length > 100 ? dto.Content.Substring(0, 100) + "..." : dto.Content;
        _unitOfWork.ChatConversations.Update(conversation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var sender = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderId = message.SenderId,
            SenderName = sender?.FullName ?? sender?.Email ?? "Unknown",
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            CreatedAt = message.CreatedAt
        };

        return Result<ChatMessageDto>.SuccessResult(messageDto);
    }

    public async Task<Result> MarkMessagesAsReadAsync(int conversationId, int userId, CancellationToken cancellationToken = default)
    {
        var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null)
        {
            return Result.FailureResult("Conversation not found");
        }

        if (conversation.EmployerId != userId && conversation.StudentId != userId)
        {
            return Result.FailureResult("Unauthorized");
        }

        var unreadMessages = await _unitOfWork.ChatMessages.FindAsync(
            m => m.ConversationId == conversationId && !m.IsRead && m.SenderId != userId,
            cancellationToken);

        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
            message.ReadAt = DateTime.UtcNow;
            _unitOfWork.ChatMessages.Update(message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult("Messages marked as read");
    }

    public async Task<Result> UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping, CancellationToken cancellationToken = default)
    {
        var conversation = await _unitOfWork.ChatConversations.GetByIdAsync(conversationId, cancellationToken);

        if (conversation == null)
        {
            return Result.FailureResult("Conversation not found");
        }

        if (conversation.EmployerId == userId)
        {
            conversation.IsEmployerTyping = isTyping;
        }
        else if (conversation.StudentId == userId)
        {
            conversation.IsStudentTyping = isTyping;
        }
        else
        {
            return Result.FailureResult("Unauthorized");
        }

        _unitOfWork.ChatConversations.Update(conversation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.SuccessResult();
    }

    public async Task<Result<int>> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default)
    {
        var conversations = await _unitOfWork.ChatConversations.FindAsync(
            c => c.EmployerId == userId || c.StudentId == userId,
            cancellationToken);

        var conversationIds = conversations.Select(c => c.Id).ToList();

        var unreadMessages = await _unitOfWork.ChatMessages.FindAsync(
            m => conversationIds.Contains(m.ConversationId) && !m.IsRead && m.SenderId != userId,
            cancellationToken);

        return Result<int>.SuccessResult(unreadMessages.Count());
    }

    private async Task<ChatConversationDto> MapToDto(ChatConversation conversation, int currentUserId, CancellationToken cancellationToken)
    {
        var employer = await _unitOfWork.Users.GetByIdAsync(conversation.EmployerId, cancellationToken);
        var student = await _unitOfWork.Users.GetByIdAsync(conversation.StudentId, cancellationToken);
        var jobPost = conversation.JobPostId.HasValue
            ? await _unitOfWork.JobPosts.GetByIdAsync(conversation.JobPostId.Value, cancellationToken)
            : null;

        var unreadCount = await _unitOfWork.ChatMessages.CountAsync(
            m => m.ConversationId == conversation.Id && !m.IsRead && m.SenderId != currentUserId,
            cancellationToken);

        return new ChatConversationDto
        {
            Id = conversation.Id,
            EmployerId = conversation.EmployerId,
            EmployerName = employer?.FullName ?? employer?.Email ?? "Unknown",
            StudentId = conversation.StudentId,
            StudentName = student?.FullName ?? student?.Email ?? "Unknown",
            JobPostId = conversation.JobPostId,
            JobPostTitle = jobPost?.Title,
            LastMessageAt = conversation.LastMessageAt,
            LastMessage = conversation.LastMessage,
            UnreadCount = unreadCount,
            CreatedAt = conversation.CreatedAt
        };
    }
}
