using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TwitterClone.Api.Endpoints;
using TwitterClone.Api.Middleware;
using TwitterClone.Application;
using TwitterClone.Application.Auth.Commands;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Profile.Commands;
using TwitterClone.Application.Tweets.Commands;
using TwitterClone.Infrastructure;
using TwitterClone.Infrastructure.Hubs;
using TwitterClone.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();
builder.Services.AddScoped<IValidator<LoginCommand>, LoginCommandValidator>();
builder.Services.AddScoped<IValidator<CreateTweetCommand>, CreateTweetCommandValidator>();
builder.Services.AddScoped<IValidator<UpdateProfileCommand>, UpdateProfileCommandValidator>();

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p
        .WithOrigins(corsOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var seeder = new DatabaseSeeder(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher>());
    await seeder.SeedAsync();
}

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

var uploadsPath = builder.Configuration["Storage:UploadsPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
});


app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapAuthEndpoints();
app.MapTweetEndpoints();
app.MapSocialEndpoints();
app.MapProfileEndpoints();
app.MapHub<TimelineHub>("/hubs/timeline");
app.MapMediaEndpoints();

app.Run();

public partial class Program { }
