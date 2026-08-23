namespace EverestFlix.Client.Models;

public class PagedResponse<T>
{
    public List<T> Items      { get; set; } = new();
    public int     Page       { get; set; }
    public int     PageSize   { get; set; }
    public long    TotalItems { get; set; }
    public int     TotalPages { get; set; }
    public bool    HasNext    { get; set; }
    public bool    HasPrevious { get; set; }
}