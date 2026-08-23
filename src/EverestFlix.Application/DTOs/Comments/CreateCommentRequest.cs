using System.ComponentModel.DataAnnotations;

namespace EverestFlix.Application.DTOs.Comments;

public class CreateCommentRequest
{
    [Required, MinLength(1), MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
}