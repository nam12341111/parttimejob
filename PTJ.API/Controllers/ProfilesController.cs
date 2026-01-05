using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.DTOs.Profile;
using PTJ.Application.Services;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IActivityLogService _activityLogService;

    public ProfilesController(IProfileService profileService, IActivityLogService activityLogService)
    {
        _profileService = profileService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Get profile by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _profileService.GetByIdAsync(id, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result);
        }

        var userId = GetUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId != 0 ? userId : null,
            "GET",
            $"/api/profiles/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Xem chi tiết profile ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Get my profile (authenticated user)
    /// </summary>
    [Authorize(Roles = "STUDENT,ADMIN")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _profileService.GetByUserIdAsync(userId, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/profiles/me",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "Student xem profile của mình");

        return Ok(result);
    }

    /// <summary>
    /// Create or update profile (Student only)
    /// </summary>
    [Authorize(Roles = "STUDENT,ADMIN")]
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] ProfileDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _profileService.CreateOrUpdateAsync(userId, dto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/profiles",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "Student tạo/cập nhật profile của mình");

        return Ok(result);
    }

    /// <summary>
    /// Delete profile (Student only)
    /// </summary>
    [Authorize(Roles = "STUDENT,ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _profileService.DeleteAsync(id, userId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "DELETE",
            $"/api/profiles/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Student xóa profile ID: {id}");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
