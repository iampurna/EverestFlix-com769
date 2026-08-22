namespace EverestFlix.Application.Common;

/// <summary>
/// Generic result wrapper (non-auth). Same pattern as AuthResult but reusable across all services.
/// </summary>
public class Result<T>
{
    public bool                  Succeeded { get; init; }
    public T?                    Value     { get; init; }
    public string?               ErrorCode { get; init; }
    public IReadOnlyList<string> Errors    { get; init; } = Array.Empty<string>();

    public static Result<T> Success(T value) =>
        new() { Succeeded = true, Value = value };

    public static Result<T> Fail(string code, params string[] errors) =>
        new() { Succeeded = false, ErrorCode = code, Errors = errors };
}