using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.DTOs.Chat;
using PTJ.Application.Services;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IActivityLogService _activityLogService;

    public ChatController(IChatService chatService, IActivityLogService activityLogService)
    {
        _chatService = chatService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Get or create a conversation between two users
    /// </summary>
    [HttpPost("conversations")]
    public async Task<IActionResult> GetOrCreateConversation([FromBody] GetOrCreateConversationDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _chatService.GetOrCreateConversationAsync(userId, dto.RecipientId, dto.JobPostId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/chat/conversations",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"User tạo/lấy conversation với User ID: {dto.RecipientId}, Job ID: {dto.JobPostId}");

        return Ok(result);
    }

    /// <summary>
    /// Get all conversations for the current user
    /// </summary>
    [HttpGet("conversations")]
    public async Task<IActionResult> GetConversations([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _chatService.GetUserConversationsAsync(userId, pageNumber, pageSize, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/chat/conversations",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            "User xem danh sách conversations");

        return Ok(result);
    }

    /// <summary>
    /// Get messages for a specific conversation
    /// </summary>
    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _chatService.GetConversationMessagesAsync(conversationId, userId, pageNumber, pageSize, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            $"/api/chat/conversations/{conversationId}/messages",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            $"User xem tin nhắn của conversation ID: {conversationId}");

        return Ok(result);
    }

    /// <summary>
    /// Send a message (alternative to SignalR)
    /// </summary>
    [HttpPost("messages")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _chatService.SendMessageAsync(userId, dto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/chat/messages",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"User gửi tin nhắn trong conversation ID: {dto.ConversationId}");

        return Ok(result);
    }

    /// <summary>
    /// Mark messages in a conversation as read
    /// </summary>
    [HttpPost("conversations/{conversationId}/read")]
    public async Task<IActionResult> MarkAsRead(int conversationId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _chatService.MarkMessagesAsReadAsync(conversationId, userId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            $"/api/chat/conversations/{conversationId}/read",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"User đánh dấu đã đọc tin nhắn trong conversation ID: {conversationId}");

        return Ok(result);
    }

    /// <summary>
    /// Get unread message count for the current user
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _chatService.GetUnreadCountAsync(userId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/chat/unread-count",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "User kiểm tra số tin nhắn chưa đọc");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
