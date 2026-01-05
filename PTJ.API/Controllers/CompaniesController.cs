using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.Common;
using PTJ.Application.DTOs.Company;
using PTJ.Application.Services;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompaniesController : ControllerBase
{
    private readonly ICompanyService _companyService;
    private readonly IActivityLogService _activityLogService;

    public CompaniesController(ICompanyService companyService, IActivityLogService activityLogService)
    {
        _companyService = companyService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Get company by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _companyService.GetByIdAsync(id, cancellationToken);

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
            $"/api/companies/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Xem chi tiết công ty ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Get all companies with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var result = await _companyService.GetAllAsync(pageNumber, pageSize, cancellationToken);

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
            "/api/companies",
            $"pageNumber={pageNumber}&pageSize={pageSize}",
            ipAddress,
            userAgent,
            200,
            0,
            "Xem danh sách công ty");

        return Ok(result);
    }

    /// <summary>
    /// Search companies by name, description, industry, or address
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? searchTerm, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] bool sortDescending = true, CancellationToken cancellationToken = default)
    {
        var parameters = new SearchParameters
        {
            SearchTerm = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortDescending = sortDescending
        };

        var result = await _companyService.SearchAsync(parameters, cancellationToken);

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
            "/api/companies/search",
            $"searchTerm={searchTerm}",
            ipAddress,
            userAgent,
            200,
            0,
            $"Tìm kiếm công ty với từ khóa: {searchTerm}");

        return Ok(result);
    }

    /// <summary>
    /// Get my company (authenticated user)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpGet("me")]
    public async Task<IActionResult> GetMyCompany(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _companyService.GetByUserIdAsync(userId, cancellationToken);

        if (!result.Success)
        {
            return NotFound(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/companies/me",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            "Employer xem thông tin công ty của mình");

        return Ok(result);
    }

    /// <summary>
    /// Submit company registration request (Any authenticated user)
    /// </summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCompanyDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _companyService.CreateAsync(userId, dto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/companies",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"User gửi yêu cầu đăng ký công ty: {dto.Name}");

        return Ok(result);
    }

    /// <summary>
    /// Update company (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCompanyDto dto, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _companyService.UpdateAsync(id, userId, dto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "PUT",
            $"/api/companies/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Employer cập nhật thông tin công ty ID: {id}");

        return Ok(result);
    }

    /// <summary>
    /// Delete company (Employer only)
    /// </summary>
    [Authorize(Roles = "EMPLOYER,ADMIN")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var result = await _companyService.DeleteAsync(id, userId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "DELETE",
            $"/api/companies/{id}",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"Employer xóa công ty ID: {id}");

        return Ok(result);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
