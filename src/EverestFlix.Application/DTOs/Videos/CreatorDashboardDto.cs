namespace EverestFlix.Application.DTOs.Videos;

public class CreatorDashboardDto
{
    public int    TotalVideos    { get; set; }
    public long   TotalViews     { get; set; }
    public double AverageRating  { get; set; }
    public int    TotalComments  { get; set; }
}