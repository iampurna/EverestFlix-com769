namespace EverestFlix.Domain.Entities;

public class Comment
{
    public int      Id        { get; set; }
    public int      VideoId   { get; set; }
    public string   UserId    { get; set; } = string.Empty;
    public string   Text      { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Reserved for Phase 10 (Azure AI Language sentiment analysis):
    // public string? Sentiment      { get; set; }
    // public double? SentimentScore { get; set; }

    // Navigation
    public Video?           Video { get; set; }
    public ApplicationUser? User  { get; set; }
}