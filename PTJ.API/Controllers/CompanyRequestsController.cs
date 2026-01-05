using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.DTOs.Company;
using PTJ.Application.Services;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class CompanyRequestsController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IActivityLogService _activityLogService;

    public CompanyRequestsController(ICompanyService companyService, IActivityLogService activityLogService)
    {
        _companyService = companyService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Get all pending company registration requests (ADMIN only)
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingRequests(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _companyService.GetPendingRequestsAsync(pageNumber, pageSize, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/companyrequests/pending",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            "Admin xem danh sách yêu cầu đăng ký công ty đang chờ duyệt");

        return Ok(result);
    }

    /// <summary>
    /// Get company registration request by ID (ADMIN only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetRequestById(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _companyService.GetRequestByIdAsync(id, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            $"/api/companyrequests/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin xem chi tiết yêu cầu đăng ký công ty ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Approve company registration request (ADMIN only)
    /// </summary>
    [HttpPost("approve")]
    public async Task<IActionResult> ApproveRequest(
        [FromBody] ApproveCompanyRequestDto dto,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetUserId();
        var result = await _companyService.ApproveRequestAsync(dto.RequestId, adminUserId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            adminUserId,
            "POST",
            "/api/companyrequests/approve",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin duyệt yêu cầu đăng ký công ty Request ID: {dto.RequestId}");

        return Ok(result);
    }

    /// <summary>
    /// Reject company registration request (ADMIN only)
    /// </summary>
    [HttpPost("reject")]
    public async Task<IActionResult> RejectRequest(
        [FromBody] RejectCompanyRequestDto dto,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetUserId();
        var result = await _companyService.RejectRequestAsync(dto.RequestId, adminUserId, dto.RejectionReason, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            adminUserId,
            "POST",
            "/api/companyrequests/reject",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Admin từ chối yêu cầu đăng ký công ty Request ID: {dto.RequestId}, lý do: {dto.RejectionReason}");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}

