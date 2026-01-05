using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.Common;
using PTJ.Application.Services;
using PTJ.Domain.Enums;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IActivityLogService _activityLogService;

    public AdminController(IAdminService adminService, IActivityLogService activityLogService)
    {
        _adminService = adminService;
        _activityLogService = activityLogService;
    }

    [HttpPost("users/{id}/lock")]
    public async Task<ActionResult<Result>> LockUser(int id)
    {
        var userId = GetUserId();
        var result = await _adminService.LockUserAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            $"/api/admin/users/{id}/lock",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin khóa tài khoản User ID: {id}");

        return Ok(result);
    }

    [HttpPost("users/{id}/unlock")]
    public async Task<ActionResult<Result>> UnlockUser(int id)
    {
        var userId = GetUserId();
        var result = await _adminService.UnlockUserAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            $"/api/admin/users/{id}/unlock",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin mở khóa tài khoản User ID: {id}");

        return Ok(result);
    }

    [HttpGet("users")]
    public async Task<ActionResult<Result<PaginatedList<object>>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var result = await _adminService.GetUsersAsync(search, pageNumber, pageSize);

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/admin/users",
            $"search={search}&pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin xem danh sách users, tìm kiếm: {search ?? "all"}");

        return Ok(result);
    }

    [HttpPut("jobs/{id}/status")]
    public async Task<ActionResult<Result>> UpdateJobStatus(int id, [FromBody] UpdateJobStatusRequest request)
    {
        var userId = GetUserId();
        var result = await _adminService.UpdateJobPostStatusAsync(id, request.Status);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "PUT",
            $"/api/admin/jobs/{id}/status",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin cập nhật trạng thái tin tuyển dụng ID: {id} thành {request.Status}");

        return Ok(result);
    }

    [HttpDelete("jobs/{id}")]
    public async Task<ActionResult<Result>> DeleteJob(int id)
    {
        var userId = GetUserId();
        var result = await _adminService.DeleteJobPostAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "DELETE",
            $"/api/admin/jobs/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin xóa tin tuyển dụng ID: {id}");

        return Ok(result);
    }

    [HttpGet("stats")]
    public async Task<ActionResult<Result<object>>> GetDashboardStats()
    {
        var userId = GetUserId();
        var result = await _adminService.GetDashboardStatsAsync();

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/admin/stats",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "Admin xem thống kê dashboard");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

public class UpdateJobStatusRequest
{
    public JobPostStatus Status { get; set; }
}
