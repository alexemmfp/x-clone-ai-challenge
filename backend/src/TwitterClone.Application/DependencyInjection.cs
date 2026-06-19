using Microsoft.Extensions.DependencyInjection;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Profile.Commands;
using TwitterClone.Application.Profile.Queries;
using TwitterClone.Application.Search.Queries;
using TwitterClone.Application.Social.Commands;
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
        services.AddScoped<GetTweetHandler>();
        services.AddScoped<GetRepliesHandler>();

        services.AddScoped<FollowHandler>();
        services.AddScoped<UnfollowHandler>();
        services.AddScoped<LikeHandler>();
        services.AddScoped<UnlikeHandler>();

        services.AddScoped<GetProfileHandler>();
        services.AddScoped<UpdateProfileHandler>();
        services.AddScoped<SearchUsersHandler>();

        return services;
    }
}
