using Microsoft.Extensions.DependencyInjection;

namespace EverestFlix.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Application services are registered in Infrastructure.DependencyInjection because
        // their implementations live there. This method stays as a stable extension point
        // for future application-only services (e.g. FluentValidation validators).
        return services;
    }
}