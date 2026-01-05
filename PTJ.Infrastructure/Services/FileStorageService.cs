using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using PTJ.Application.Common;
using PTJ.Application.Services;
using PTJ.Domain.Common;
using PTJ.Domain.Entities;
using PTJ.Domain.Interfaces;

namespace PTJ.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _uploadPath;

    private static readonly HashSet<string> AllowedFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "avatars",
        "cvs",
        "logos",
        "certificates",
        "general"
    };

    public LocalFileStorageService(IUnitOfWork unitOfWork, IHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _uploadPath = Path.Combine(environment.ContentRootPath, "uploads");

        // Ensure upload directory exists
        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<Result<string>> UploadFileAsync(IFormFile file, string folder, int userId, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            return Result<string>.FailureResult("No file provided");
        }

        // Validate folder whitelist
        if (!AllowedFolders.Contains(folder))
        {
            return Result<string>.FailureResult($"Folder '{folder}' is not allowed. Allowed folders: {string.Join(", ", AllowedFolders)}");
        }

        // Validate file size (max 10MB)
        const long maxFileSize = 10 * 1024 * 1024;
        if (file.Length > maxFileSize)
        {
            return Result<string>.FailureResult("File size exceeds 10MB limit");
        }

        // Validate file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return Result<string>.FailureResult($"File type {fileExtension} is not allowed");
        }

        try
        {
            // Create folder path with path traversal protection
            var folderPath = Path.Combine(_uploadPath, folder);
            var fullFolderPath = Path.GetFullPath(folderPath);

            // Verify the resolved path is still within upload directory
            if (!fullFolderPath.StartsWith(_uploadPath, StringComparison.OrdinalIgnoreCase))
            {
                return Result<string>.FailureResult("Invalid folder path detected");
            }

            if (!Directory.Exists(fullFolderPath))
            {
                Directory.CreateDirectory(fullFolderPath);
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(fullFolderPath, uniqueFileName);
            var fullFilePath = Path.GetFullPath(filePath);

            // Final path traversal check
            if (!fullFilePath.StartsWith(_uploadPath, StringComparison.OrdinalIgnoreCase))
            {
                return Result<string>.FailureResult("Invalid file path detected");
            }

            // Save file
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Create file URL
            var fileUrl = $"/uploads/{folder}/{uniqueFileName}";

            // Save file metadata to database
            var fileEntity = new FileEntity
            {
                FileName = file.FileName,
                FileUrl = fileUrl,
                FileType = folder,
                FileSize = file.Length,
                ContentType = file.ContentType,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Files.AddAsync(fileEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<string>.SuccessResult(fileUrl, "File uploaded successfully");
        }
        catch (Exception ex)
        {
            return Result<string>.FailureResult($"File upload failed: {ex.Message}");
        }
    }

    public async Task<Result> DeleteFileAsync(string fileUrl, int userId, IEnumerable<string> userRoles, CancellationToken cancellationToken = default)
    {
        try
        {
            // Find file in database
            var fileEntity = await _unitOfWork.Files.FirstOrDefaultAsync(
                f => f.FileUrl == fileUrl,
                cancellationToken);

            if (fileEntity == null)
            {
                return Result.FailureResult("File not found in database");
            }

            // Check ownership: only file owner or ADMIN can delete
            var isAdmin = userRoles.Contains(RoleConstants.Admin);
            var isOwner = fileEntity.UploadedBy == userId;

            if (!isAdmin && !isOwner)
            {
                return Result.FailureResult("Unauthorized: You can only delete your own files");
            }

            // Construct safe file path with traversal protection
            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePath = Path.Combine(_uploadPath, relativePath);
            var fullFilePath = Path.GetFullPath(filePath);

            // Verify the resolved path is within upload directory
            if (!fullFilePath.StartsWith(_uploadPath, StringComparison.OrdinalIgnoreCase))
            {
                return Result.FailureResult("Invalid file path detected");
            }

            // Delete physical file
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
            }

            // Delete from database
            _unitOfWork.Files.Remove(fileEntity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.SuccessResult("File deleted successfully");
        }
        catch (Exception ex)
        {
            return Result.FailureResult($"File deletion failed: {ex.Message}");
        }
    }

    public async Task<Result<byte[]>> DownloadFileAsync(string fileUrl, int userId, IEnumerable<string> userRoles, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if file exists in database
            var fileEntity = await _unitOfWork.Files.FirstOrDefaultAsync(
                f => f.FileUrl == fileUrl,
                cancellationToken);

            if (fileEntity == null)
            {
                return Result<byte[]>.FailureResult("File not found");
            }

            // Check ownership: only file owner or ADMIN can download
            var isAdmin = userRoles.Contains(RoleConstants.Admin);
            var isOwner = fileEntity.UploadedBy == userId;

            if (!isAdmin && !isOwner)
            {
                return Result<byte[]>.FailureResult("Unauthorized: You can only download your own files");
            }

            // Construct safe file path with traversal protection
            var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var filePath = Path.Combine(_uploadPath, relativePath);
            var fullFilePath = Path.GetFullPath(filePath);

            // Verify the resolved path is within upload directory
            if (!fullFilePath.StartsWith(_uploadPath, StringComparison.OrdinalIgnoreCase))
            {
                return Result<byte[]>.FailureResult("Invalid file path detected");
            }

            if (!File.Exists(fullFilePath))
            {
                return Result<byte[]>.FailureResult("Physical file not found");
            }

            var fileBytes = await File.ReadAllBytesAsync(fullFilePath, cancellationToken);

            return Result<byte[]>.SuccessResult(fileBytes);
        }
        catch (Exception ex)
        {
            return Result<byte[]>.FailureResult($"File download failed: {ex.Message}");
        }
    }
}
