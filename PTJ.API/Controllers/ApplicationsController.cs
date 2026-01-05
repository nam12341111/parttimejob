using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.DTOs.Application;
using PTJ.Application.Services;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IProfileService _profileService;
    private readonly IActivityLogService _activityLogService;

    public ApplicationsController(IApplicationService applicationService, IProfileService profileService, IActivityLogService activityLogService)
    {
        _applicationService = applicationService;
        _profileService = profileService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Get application by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _applicationService.GetByIdAsync(id, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId != 0 ? userId : null,
            "GET",
            $"/api/applications/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Xem chi tiết đơn ứng tuyển ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Get applications by job post ID (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpGet("job/{jobPostId}")]
    public async Task<IActionResult> GetByJobPostId(int jobPostId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _applicationService.GetByJobPostIdAsync(jobPostId, pageNumber, pageSize, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            $"/api/applications/job/{jobPostId}",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            $"Employer xem danh sách đơn ứng tuyển của Job ID: {jobPostId}");

        return Ok(result);
    }

    /// <summary>
    /// Get my applications (Student only)
    /// </summary>
    [Authorize(Roles = "STUDENT,ADMIN")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyApplications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();

        var profileResult = await _profileService.GetByUserIdAsync(userId, cancellationToken);
        if (!profileResult.Success || profileResult.Data == null)
        {
            return BadRequest(new { Success = false, Message = "Profile not found" });
        }

        var result = await _applicationService.GetByProfileIdAsync(profileResult.Data.Id, pageNumber, pageSize, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/applications/me",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            "Student xem danh sách đơn ứng tuyển của mình");

        return Ok(result);
    }

    /// <summary>
    /// Create application (Student only)
    /// </summary>
    [Authorize(Roles = "STUDENT,ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateApplicationDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _applicationService.CreateAsync(userId, dto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        // Log activity
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/applications",
            null,
            ipAddress,
            userAgent,
            201,
            0,
            $"Application submitted for Job ID: {dto.JobPostId}");

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update application status (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateApplicationStatusDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _applicationService.UpdateStatusAsync(id, dto.StatusId, userId, dto.Notes, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "PATCH",
            $"/api/applications/{id}/status",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Employer cập nhật trạng thái đơn ứng tuyển ID: {id} thành Status ID: {dto.StatusId}");

        return Ok(result);
    }

    /// <summary>
    /// Withdraw application (Student only)
    /// </summary>
    [Authorize(Roles = "STUDENT,ADMIN")]
    [HttpPost("{id}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _applicationService.WithdrawAsync(id, userId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            $"/api/applications/{id}/withdraw",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Student rút đơn ứng tuyển ID: {id}");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
