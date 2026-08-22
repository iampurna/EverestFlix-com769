namespace EverestFlix.Application.Common;

public class PagedResponse<T>
{
    public IReadOnlyList<T> Items       { get; init; } = Array.Empty<T>();
    public int              Page        { get; init; }
    public int              PageSize    { get; init; }
    public long             TotalItems  { get; init; }
    public int              TotalPages  => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
    public bool             HasNext     => Page < TotalPages;
    public bool             HasPrevious => Page > 1;
}