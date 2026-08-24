using EverestFlix.Application.Common;
using EverestFlix.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Storage;

public class LocalVideoStorageService : IVideoStorageService
{
    private const long MaxVideoBytes =
        100_000_000;


    private readonly string _localRoot;

    private readonly string _publicBaseUrl;

    private readonly ILogger<LocalVideoStorageService> _logger;


    public LocalVideoStorageService(
        IConfiguration configuration,
        ILogger<LocalVideoStorageService> logger)
    {
        _localRoot =
            configuration["Storage:LocalRoot"]
            ?? throw new InvalidOperationException(
                "Storage:LocalRoot missing.");


        _publicBaseUrl =
            configuration["Storage:PublicBaseUrl"]
            ?? throw new InvalidOperationException(
                "Storage:PublicBaseUrl missing.");


        _logger =
            logger;


        Directory.CreateDirectory(
            _localRoot);
    }


    public async Task<string> SaveAsync(
        VideoUpload upload,
        CancellationToken ct = default)
    {
        ValidateUpload(upload);


        // Never trust the original filename for storage.
        // A generated name prevents collisions and path issues.
        var fileName =
            $"{Guid.NewGuid():N}.mp4";


        var fullPath =
            Path.Combine(
                _localRoot,
                fileName);


        await using (
            var fs = new FileStream(
                fullPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None))
        {
            await upload.Content.CopyToAsync(
                fs,
                ct);
        }


        var publicUrl =
            $"{_publicBaseUrl.TrimEnd('/')}/{fileName}";


        _logger.LogInformation(
            "Saved MP4 video: {FileName} ({Size} bytes) -> {Url}",
            fileName,
            upload.Length,
            publicUrl);


        return publicUrl;
    }


    public Task DeleteAsync(
        string url,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return Task.CompletedTask;
        }


        var fileName =
            Path.GetFileName(url);


        var fullPath =
            Path.Combine(
                _localRoot,
                fileName);


        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);


                _logger.LogInformation(
                    "Deleted video: {FileName}",
                    fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to delete video file: {FullPath}",
                fullPath);
        }


        return Task.CompletedTask;
    }


    private static void ValidateUpload(
        VideoUpload upload)
    {
        if (upload is null)
        {
            throw new InvalidOperationException(
                "Video file is required.");
        }


        if (upload.Length <= 0)
        {
            throw new InvalidOperationException(
                "Video file is empty.");
        }


        if (upload.Length > MaxVideoBytes)
        {
            throw new InvalidOperationException(
                "Video must not exceed 100 MB.");
        }


        var extension =
            Path.GetExtension(
                upload.FileName);


        if (!string.Equals(
                extension,
                ".mp4",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only MP4 video files are supported.");
        }


        var mediaType =
            upload.ContentType?
                .Split(';', 2)[0]
                .Trim();


        if (!string.Equals(
                mediaType,
                "video/mp4",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only video/mp4 content is supported.");
        }


        if (!upload.Content.CanRead)
        {
            throw new InvalidOperationException(
                "Video stream cannot be read.");
        }
    }
}