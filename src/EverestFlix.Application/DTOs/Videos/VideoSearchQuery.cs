namespace EverestFlix.Application.DTOs.Videos;

public class VideoSearchQuery
{
    public string? Q        { get; set; }
    public string? Genre    { get; set; }
    public int     Page     { get; set; } = 1;
    public int     PageSize { get; set; } = 12;
}