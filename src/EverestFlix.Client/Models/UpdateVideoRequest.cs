namespace EverestFlix.Client.Models;

public class UpdateVideoRequest
{
    public string Title { get; set; } =
        string.Empty;

    public string? Description { get; set; }

    public string Publisher { get; set; } =
        string.Empty;

    public string Producer { get; set; } =
        string.Empty;

    public string Genre { get; set; } =
        string.Empty;

    public AgeRating AgeRating { get; set; }
}