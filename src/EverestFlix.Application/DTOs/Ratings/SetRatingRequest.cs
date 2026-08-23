using System.ComponentModel.DataAnnotations;

namespace EverestFlix.Application.DTOs.Ratings;

public class SetRatingRequest
{
    [Range(1, 5)]
    public int Value { get; set; }
}