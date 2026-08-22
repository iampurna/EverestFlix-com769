using EverestFlix.Domain.Enums;

namespace EverestFlix.Domain.Entities;

public class Video
{
    public int      Id            { get; set; }
    public string   Title         { get; set; } = string.Empty;
    public string?  Description   { get; set; }
    public string   Publisher     { get; set; } = string.Empty;
    public string   Producer      { get; set; } = string.Empty;
    public string   Genre         { get; set; } = string.Empty;
    public AgeRating AgeRating    { get; set; }
    public string   VideoUrl      { get; set; } = string.Empty;
    public string?  ThumbnailUrl  { get; set; }
    public string   CreatorId     { get; set; } = string.Empty;
    public DateTime CreatedAt     { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt    { get; set; }
    public long     ViewCount     { get; set; }
    public bool     IsPublished   { get; set; } = true;

    // Navigation
    public ApplicationUser? Creator  { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Rating>  Ratings  { get; set; } = new List<Rating>();
}