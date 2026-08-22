namespace EverestFlix.Application.Interfaces;

public interface ICurrentUserService
{
    string?               UserId          { get; }
    string?               Email           { get; }
    IReadOnlyList<string> Roles           { get; }
    bool                  IsAuthenticated { get; }
    bool                  IsInRole(string role);
}