namespace EverestFlix.Domain.Entities;

public class Rating
{
    public int      Id        { get; set; }
    public int      VideoId   { get; set; }
    public string   UserId    { get; set; } = string.Empty;
    public int      Value     { get; set; }   // 1..5, enforced at Application layer in Phase 4
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Video?           Video { get; set; }
    public ApplicationUser? User  { get; set; }
}