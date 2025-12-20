using PTJ.Application.Common;
using PTJ.Application.DTOs.Chat;

namespace PTJ.Application.Services;

public interface IChatService
{
    Task<Result<ChatConversationDto>> GetOrCreateConversationAsync(int userId, int recipientId, int? jobPostId = null, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<ChatConversationDto>>> GetUserConversationsAsync(int userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<PaginatedList<ChatMessageDto>>> GetConversationMessagesAsync(int conversationId, int userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<ChatMessageDto>> SendMessageAsync(int userId, SendMessageDto dto, CancellationToken cancellationToken = default);
    Task<Result> MarkMessagesAsReadAsync(int conversationId, int userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateTypingStatusAsync(int conversationId, int userId, bool isTyping, CancellationToken cancellationToken = default);
    Task<Result<int>> GetUnreadCountAsync(int userId, CancellationToken cancellationToken = default);
}
