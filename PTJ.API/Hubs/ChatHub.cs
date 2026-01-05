using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PTJ.Application.DTOs.Chat;
using PTJ.Application.Services;

namespace PTJ.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatService _chatService;

    public ChatHub(IChatService chatService)
    {
        _chatService = chatService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId > 0)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user-{userId}");
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(SendMessageDto dto)
    {
        var userId = GetUserId();
        var result = await _chatService.SendMessageAsync(userId, dto, Context.ConnectionAborted);

        if (result.Success && result.Data != null)
        {
            // Send to sender
            await Clients.Caller.SendAsync("ReceiveMessage", result.Data);

            // Determine recipient
            var conversation = (await _chatService.GetOrCreateConversationAsync(
                userId,
                dto.RecipientId ?? 0,
                dto.JobPostId,
                Context.ConnectionAborted)).Data;

            if (conversation != null)
            {
                int recipientId = conversation.EmployerId == userId
                    ? conversation.StudentId
                    : conversation.EmployerId;

                // Send to recipient
                await Clients.Group($"user-{recipientId}").SendAsync("ReceiveMessage", result.Data);
            }
        }
        else
        {
            await Clients.Caller.SendAsync("Error", result.Message);
        }
    }

    public async Task MarkAsRead(int conversationId)
    {
        var userId = GetUserId();
        var result = await _chatService.MarkMessagesAsReadAsync(conversationId, userId, Context.ConnectionAborted);

        if (result.Success)
        {
            await Clients.Caller.SendAsync("MessagesMarkedAsRead", conversationId);
        }
    }

    public async Task UpdateTyping(int conversationId, bool isTyping)
    {
        var userId = GetUserId();
        var result = await _chatService.UpdateTypingStatusAsync(conversationId, userId, isTyping, Context.ConnectionAborted);

        if (result.Success)
        {
            // Notify the other user in the conversation
            await Clients.Group($"conversation-{conversationId}").SendAsync("UserTyping", userId, isTyping);
        }
    }

    public async Task JoinConversation(int conversationId)
    {
        var userId = GetUserId();
        
        // Check if user is member of this conversation
        var isMember = await _chatService.IsUserInConversationAsync(conversationId, userId, Context.ConnectionAborted);
        
        if (!isMember)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized: You are not a member of this conversation");
            return;
        }
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        await Clients.Caller.SendAsync("JoinedConversation", conversationId);
    }

    public async Task LeaveConversation(int conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation-{conversationId}");
        await Clients.Caller.SendAsync("LeftConversation", conversationId);
    }

    private int GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
