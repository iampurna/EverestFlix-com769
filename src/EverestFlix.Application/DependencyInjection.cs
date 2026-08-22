using Microsoft.Extensions.DependencyInjection;

namespace EverestFlix.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services (validators, orchestrators, etc.) register here in Phase 4+.
        // Phase 3: layer is contracts-only, so nothing to register yet — the method
        // exists so Program.cs can call it and stay stable across phases.
        return services;
    }
}