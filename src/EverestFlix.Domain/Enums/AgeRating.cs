namespace EverestFlix.Domain.Enums;

/// <summary>
/// UK-style age ratings for uploaded videos.
/// Numeric identifiers use word-form because C# enum members cannot begin with a digit.
/// </summary>
public enum AgeRating
{
    U        = 0,
    PG       = 1,
    Twelve   = 12,
    Fifteen  = 15,
    Eighteen = 18
}