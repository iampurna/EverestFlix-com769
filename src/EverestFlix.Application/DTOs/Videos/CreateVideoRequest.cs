using System.ComponentModel.DataAnnotations;
using EverestFlix.Application.Common;
using EverestFlix.Domain.Enums;

namespace EverestFlix.Application.DTOs.Videos;

public class CreateVideoRequest
{
    [Required, MaxLength(200)] public string      Title       { get; set; } = string.Empty;
    [MaxLength(2000)]          public string?     Description { get; set; }
    [Required, MaxLength(150)] public string      Publisher   { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string      Producer    { get; set; } = string.Empty;
    [Required, MaxLength(80)]  public string      Genre       { get; set; } = string.Empty;
    [Required]                 public AgeRating   AgeRating   { get; set; }
    [Required]                 public VideoUpload VideoFile   { get; set; } = default!;
}