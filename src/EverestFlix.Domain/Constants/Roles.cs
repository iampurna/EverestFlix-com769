namespace EverestFlix.Domain.Constants;

public static class Roles
{
    public const string Consumer = nameof(Consumer);
    public const string Creator  = nameof(Creator);
    public const string Admin    = nameof(Admin);

    public static IReadOnlyList<string> All { get; } = new[] { Consumer, Creator, Admin };
}