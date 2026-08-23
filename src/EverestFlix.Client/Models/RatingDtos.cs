namespace EverestFlix.Client.Models;

public class RatingSummary
{
    public double Average  { get; set; }
    public int    Count    { get; set; }
    public int?   MyRating { get; set; }
}

public class SetRatingRequest
{
    public int Value { get; set; }
}