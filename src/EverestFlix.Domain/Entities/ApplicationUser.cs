using Microsoft.AspNetCore.Identity;

namespace EverestFlix.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string  FullName        { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt      { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Video>   Videos   { get; set; } = new List<Video>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Rating>  Ratings  { get; set; } = new List<Rating>();
}