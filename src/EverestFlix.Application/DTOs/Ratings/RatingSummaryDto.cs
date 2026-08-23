namespace EverestFlix.Application.DTOs.Ratings;

public class RatingSummaryDto
{
    public double Average   { get; set; }
    public int    Count     { get; set; }
    public int?   MyRating  { get; set; }  // null if not authenticated or not yet rated
}