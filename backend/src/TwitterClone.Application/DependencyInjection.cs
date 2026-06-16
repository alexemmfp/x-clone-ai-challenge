using Microsoft.Extensions.DependencyInjection;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Tweets.Commands;
using TwitterClone.Application.Tweets.Queries;

namespace TwitterClone.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterHandler>();
        services.AddScoped<LoginHandler>();
        services.AddScoped<RefreshHandler>();
        services.AddScoped<LogoutHandler>();

        services.AddScoped<CreateTweetHandler>();
        services.AddScoped<DeleteTweetHandler>();
        services.AddScoped<GetTimelineHandler>();

        return services;
    }
}
