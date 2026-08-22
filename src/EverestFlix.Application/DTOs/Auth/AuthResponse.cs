namespace EverestFlix.Application.DTOs.Auth;

public class AuthResponse
{
    public string       Token     { get; set; } = string.Empty;
    public DateTime     ExpiresAt { get; set; }
    public UserInfoDto  User      { get; set; } = new();
}