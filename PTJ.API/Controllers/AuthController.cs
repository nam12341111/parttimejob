using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.DTOs.Auth;
using PTJ.Application.Services;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IActivityLogService _activityLogService;

    public AuthController(IAuthService authService, IActivityLogService activityLogService)
    {
        _authService = authService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Register a new user (STUDENT or EMPLOYER only)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        // Log activity
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            result.Data?.UserId, 
            "POST", 
            "/api/auth/register", 
            null, 
            ipAddress, 
            userAgent, 
            200, 
            0, 
            $"User registered as {result.Data?.Role}");

        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var result = await _authService.LoginAsync(dto, ipAddress, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        // Log activity
        var userAgent = Request.Headers["User-Agent"].ToString();
        
        await _activityLogService.LogActivityAsync(
            result.Data?.UserId, 
            "POST", 
            "/api/auth/login", 
            null, 
            ipAddress, 
            userAgent, 
            200, 
            0, 
            "User logged in");

        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var result = await _authService.RefreshTokenAsync(dto.RefreshToken, ipAddress, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            result.Data?.UserId,
            "POST",
            "/api/auth/refresh",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "User làm mới token");

        return Ok(result);
    }

    /// <summary>
    /// Revoke refresh token (logout) - Requires authentication
    /// </summary>
    [Authorize]
    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var result = await _authService.RevokeTokenAsync(dto.RefreshToken, userId, ipAddress, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/auth/revoke",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "User đăng xuất (revoke token)");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
