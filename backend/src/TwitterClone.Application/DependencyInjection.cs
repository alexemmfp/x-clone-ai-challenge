using Microsoft.Extensions.DependencyInjection;

namespace TwitterClone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
