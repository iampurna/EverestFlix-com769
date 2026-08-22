using EverestFlix.Domain.Enums;

namespace EverestFlix.Application.DTOs.Videos;

public class VideoSummaryDto
{
    public int       Id            { get; set; }
    public string    Title         { get; set; } = string.Empty;
    public string    Publisher     { get; set; } = string.Empty;
    public string    Genre         { get; set; } = string.Empty;
    public AgeRating AgeRating     { get; set; }
    public string    VideoUrl      { get; set; } = string.Empty;
    public string?   ThumbnailUrl  { get; set; }
    public string    CreatorId     { get; set; } = string.Empty;
    public string    CreatorName   { get; set; } = string.Empty;
    public DateTime  CreatedAt     { get; set; }
    public long      ViewCount     { get; set; }
}