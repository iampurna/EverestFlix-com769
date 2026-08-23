namespace EverestFlix.Client.Models;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email    { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string       Token     { get; set; } = string.Empty;
    public DateTime     ExpiresAt { get; set; }
    public UserInfo     User      { get; set; } = new();
}

public class UserInfo
{
    public string       Id       { get; set; } = string.Empty;
    public string       Email    { get; set; } = string.Empty;
    public string       FullName { get; set; } = string.Empty;
    public List<string> Roles    { get; set; } = new();
}