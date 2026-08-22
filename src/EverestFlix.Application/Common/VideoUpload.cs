namespace EverestFlix.Application.Common;

/// <summary>
/// Transport-neutral upload payload. The API controller adapts IFormFile → VideoUpload
/// at the boundary so the Application layer never depends on ASP.NET Core types.
/// </summary>
public class VideoUpload
{
    public required Stream Content     { get; init; }
    public required string FileName    { get; init; }
    public required string ContentType { get; init; }
    public required long   Length      { get; init; }
}