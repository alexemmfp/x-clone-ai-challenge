using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Profile;

public class ProfileEndpointsTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string token, string username)> RegisterAsync(string suffix)
    {
        var username = $"prof{suffix}";
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"prof{suffix}@example.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return (body.GetProperty("accessToken").GetString()!, username);
    }

    [Fact]
    public async Task GetProfile_ExistingUser_Returns200()
    {
        var (_, username) = await RegisterAsync("get");

        var resp = await _client.GetAsync($"/api/users/{username}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetProperty("username").GetString().Should().Be(username);
    }

    [Fact]
    public async Task GetProfile_NonExistentUser_Returns422()
    {
        var resp = await _client.GetAsync("/api/users/doesnotexist99999");
        resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateProfile_ValidRequest_Returns200WithBio()
    {
        var (token, _) = await RegisterAsync("upd");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.PatchAsJsonAsync("/api/me", new { bio = "My new bio", avatarUrl = (string?)null });

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetProperty("bio").GetString().Should().Be("My new bio");
    }

    [Fact]
    public async Task SearchUsers_MatchingTerm_Returns200WithResults()
    {
        var (token, username) = await RegisterAsync("srch");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.GetAsync($"/api/search/users?q=prof");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetArrayLength().Should().BeGreaterThan(0);
    }
}
