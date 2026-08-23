namespace EverestFlix.Client.Models;

public class CommentDto
{
    public int      Id        { get; set; }
    public int      VideoId   { get; set; }
    public string   UserId    { get; set; } = string.Empty;
    public string   UserName  { get; set; } = string.Empty;
    public string   Text      { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequest
{
    public string Text { get; set; } = string.Empty;
}