using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.Common;
using PTJ.Application.DTOs.JobPost;
using PTJ.Application.Services;
using PTJ.Domain.Enums;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobPostsController : ControllerBase
{
    private readonly IJobPostService _jobPostService;
    private readonly IActivityLogService _activityLogService;

    public JobPostsController(IJobPostService jobPostService, IActivityLogService activityLogService)
    {
        _jobPostService = jobPostService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Get job post by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _jobPostService.GetByIdAsync(id, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result);
        }

        await _jobPostService.IncrementViewCountAsync(id, cancellationToken);

        var userId = GetUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId != 0 ? userId : null,
            "GET",
            $"/api/jobposts/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Xem chi tiết tin tuyển dụng ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Get all active job posts with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _jobPostService.GetAllAsync(pageNumber, pageSize, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var userId = GetUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId != 0 ? userId : null,
            "GET",
            "/api/jobposts",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            "Xem danh sách tin tuyển dụng");

        return Ok(result);
    }

    /// <summary>
    /// Search job posts
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _jobPostService.SearchAsync(parameters, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var userId = GetUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId != 0 ? userId : null,
            "GET",
            "/api/jobposts/search",
            $"searchTerm={parameters.SearchTerm}",
            ipAddress,
            userAgent,
            200,
            0,
            $"Tìm kiếm tin tuyển dụng với từ khóa: {parameters.SearchTerm}");

        return Ok(result);
    }

    /// <summary>
    /// Get job posts by company ID
    /// </summary>
    [HttpGet("company/{companyId}")]
    public async Task<IActionResult> GetByCompanyId(int companyId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _jobPostService.GetByCompanyIdAsync(companyId, pageNumber, pageSize, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var userId = GetUserId();
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId != 0 ? userId : null,
            "GET",
            $"/api/jobposts/company/{companyId}",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            $"Xem danh sách tin tuyển dụng của công ty ID: {companyId}");

        return Ok(result);
    }

    /// <summary>
    /// Create a new job post (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateJobPostDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _jobPostService.CreateAsync(userId, dto, cancellationToken);

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
            "/api/jobposts",
            null,
            ipAddress,
            userAgent,
            201,
            0,
            $"Job posted: {dto.Title}");

        return CreatedAtAction(nameof(GetById), new { id = result.Data?.Id }, result);
    }

    /// <summary>
    /// Update job post (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateJobPostDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _jobPostService.UpdateAsync(id, userId, dto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "PUT",
            $"/api/jobposts/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Employer cập nhật tin tuyển dụng ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Delete job post (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _jobPostService.DeleteAsync(id, userId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "DELETE",
            $"/api/jobposts/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Employer xóa tin tuyển dụng ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Change job post status (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> ChangeStatus(int id, [FromBody] JobPostStatus status, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _jobPostService.ChangeStatusAsync(id, userId, status, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "PATCH",
            $"/api/jobposts/{id}/status",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Employer thay đổi trạng thái tin tuyển dụng ID: {id} thành {status}");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
