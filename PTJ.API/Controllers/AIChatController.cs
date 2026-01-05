using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.Services;
using System.Security.Claims;

namespace PTJ.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AIChatController : ControllerBase
{
    private readonly IAIChatService _aiChatService;
    private readonly IActivityLogService _activityLogService;

    public AIChatController(IAIChatService aiChatService, IActivityLogService activityLogService)
    {
        _aiChatService = aiChatService;
        _activityLogService = activityLogService;
    }

    [HttpPost("message")]
    public async Task<IActionResult> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message cannot be empty");

        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }

        var result = await _aiChatService.ChatAsync(userId, request.Message, HttpContext.RequestAborted);

        if (!result.Success)
            return BadRequest(new { errors = result.Message ?? "Unknown error" });

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/aichat/message",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"User gửi tin nhắn AI: {request.Message.Substring(0, Math.Min(50, request.Message.Length))}...");

        return Ok(new { response = result.Data });
    }

    [HttpPost("restart")]
    public async Task<IActionResult> RestartConversation()
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }

        var result = await _aiChatService.RestartSessionAsync(userId, HttpContext.RequestAborted);

        if (!result.Success)
            return BadRequest(new { errors = result.Message ?? "Failed to restart conversation" });

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/aichat/restart",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "User khởi động lại phiên chat AI");

        return Ok(new { message = result.Message });
    }
}

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
}
