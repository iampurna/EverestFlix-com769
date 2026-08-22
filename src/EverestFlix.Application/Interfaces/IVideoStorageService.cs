using EverestFlix.Application.Common;

namespace EverestFlix.Application.Interfaces;

public interface IVideoStorageService
{
    /// <summary>
    /// Persists the uploaded content and returns a URL suitable for embedding in <video src="...">.
    /// May be a relative URL (local dev) or absolute (Azure Blob).
    /// </summary>
    Task<string> SaveAsync(VideoUpload upload, CancellationToken ct = default);

    /// <summary>
    /// Deletes the stored file. Idempotent — missing files must not throw.
    /// </summary>
    Task DeleteAsync(string url, CancellationToken ct = default);
}