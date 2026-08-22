using EverestFlix.Application.Common;
using EverestFlix.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EverestFlix.Infrastructure.Storage;

public class LocalVideoStorageService : IVideoStorageService
{
    private readonly string                              _localRoot;
    private readonly string                              _publicBaseUrl;
    private readonly ILogger<LocalVideoStorageService>   _logger;

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm", ".mov" };

    public LocalVideoStorageService(
        IConfiguration configuration,
        ILogger<LocalVideoStorageService> logger)
    {
        _localRoot = configuration["Storage:LocalRoot"]
            ?? throw new InvalidOperationException("Storage:LocalRoot missing.");
        _publicBaseUrl = configuration["Storage:PublicBaseUrl"]
            ?? throw new InvalidOperationException("Storage:PublicBaseUrl missing.");
        _logger = logger;

        Directory.CreateDirectory(_localRoot);
    }

    public async Task<string> SaveAsync(VideoUpload upload, CancellationToken ct = default)
    {
        if (upload is null || upload.Length == 0)
            throw new InvalidOperationException("Video file is empty.");

        var extension = Path.GetExtension(upload.FileName);
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Unsupported video type '{extension}'. Allowed: {string.Join(", ", AllowedExtensions)}");

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_localRoot, fileName);

        await using (var fs = File.Create(fullPath))
        {
            await upload.Content.CopyToAsync(fs, ct);
        }

        var publicUrl = $"{_publicBaseUrl.TrimEnd('/')}/{fileName}";
        _logger.LogInformation("Saved video: {FileName} ({Size} bytes) → {Url}", fileName, upload.Length, publicUrl);
        return publicUrl;
    }

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url)) return Task.CompletedTask;

        var fileName = Path.GetFileName(url);
        var fullPath = Path.Combine(_localRoot, fileName);

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted video: {FileName}", fileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete video file: {FullPath}", fullPath);
        }

        return Task.CompletedTask;
    }
}