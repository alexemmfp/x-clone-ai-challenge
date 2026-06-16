using Microsoft.Extensions.DependencyInjection;
using TwitterClone.Application.Auth.Commands;

namespace TwitterClone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();

        return services;
    }
}
