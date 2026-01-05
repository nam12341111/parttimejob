using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PTJ.Application.DTOs.File;
using PTJ.Application.Services;

namespace PTJ.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;
    private readonly IActivityLogService _activityLogService;

    public FilesController(IFileStorageService fileStorageService, IActivityLogService activityLogService)
    {
        _fileStorageService = fileStorageService;
        _activityLogService = activityLogService;
    }

    /// <summary>
    /// Upload a file (avatar, resume, certificate, logo)
    /// </summary>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] FileUploadRequest request, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var result = await _fileStorageService.UploadFileAsync(request.File, request.Folder ?? "general", userId, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "POST",
            "/api/files/upload",
            null,
            ipAddress,
            userAgent,
            200,
            0,
            $"User upload file: {request.File.FileName} vào folder: {request.Folder ?? "general"}");

        return Ok(result);
    }

    /// <summary>
    /// Delete a file
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] string fileUrl, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userRoles = GetUserRoles();
        var result = await _fileStorageService.DeleteFileAsync(fileUrl, userId, userRoles, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "DELETE",
            "/api/files",
            $"fileUrl={fileUrl}",
            ipAddress,
            userAgent,
            200,
            0,
            $"User xóa file: {fileUrl}");

        return Ok(result);
    }

    /// <summary>
    /// Download a file (requires authentication)
    /// </summary>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string fileUrl, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        var userRoles = GetUserRoles();
        var result = await _fileStorageService.DownloadFileAsync(fileUrl, userId, userRoles, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = Request.Headers["User-Agent"].ToString();
        await _activityLogService.LogActivityAsync(
            userId,
            "GET",
            "/api/files/download",
            $"fileUrl={fileUrl}",
            ipAddress,
            userAgent,
            200,
            0,
            $"Download file: {fileUrl}");

        var extension = Path.GetExtension(fileUrl).ToLower();
        var contentType = extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => "application/octet-stream"
        };

        var fileName = Path.GetFileName(fileUrl);
        return File(result.Data!, contentType, fileName);
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }

    private List<string> GetUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }
}
