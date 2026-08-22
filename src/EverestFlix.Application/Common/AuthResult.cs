namespace EverestFlix.Application.Common;

/// <summary>
/// Result wrapper for auth operations. Expected failures (bad password, duplicate email)
/// return a failed result instead of throwing.
/// </summary>
public class AuthResult<T> where T : class
{
    public bool                    Succeeded { get; init; }
    public T?                      Value     { get; init; }
    public string?                 ErrorCode { get; init; }
    public IReadOnlyList<string>   Errors    { get; init; } = Array.Empty<string>();

    public static AuthResult<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static AuthResult<T> Fail(string code, params string[] errors) =>
        new() { Succeeded = false, ErrorCode = code, Errors = errors };
}