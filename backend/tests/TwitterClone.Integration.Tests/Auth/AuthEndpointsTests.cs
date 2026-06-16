using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Auth;

public class AuthEndpointsTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidRequest_Returns200WithAccessToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "newuser",
            email = "new@example.com",
            password = "Password123!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ParseBodyAsync(response);
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("username").GetString().Should().Be("newuser");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "user1",
            email = "dup@example.com",
            password = "Password123!",
        });

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "user2",
            email = "dup@example.com",
            password = "Password123!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithAccessToken()
    {
        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "loginuser",
            email = "login@example.com",
            password = "Password123!",
        });

        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "login@example.com",
            password = "Password123!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await ParseBodyAsync(response);
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_InvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@example.com",
            password = "wrongpassword",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidCookie_Returns200WithNewToken()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = "refreshuser",
            email = "refresh@example.com",
            password = "Password123!",
        });

        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await _client.PostAsync("/api/auth/refresh", null);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await ParseBodyAsync(refreshResponse);
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    private static async Task<JsonElement> ParseBodyAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<JsonElement>(json);
    }
}
