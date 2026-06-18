using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Social;

public class SocialEndpointsTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string token, string username)> RegisterAsync(string suffix)
    {
        var username = $"social{suffix}";
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"social{suffix}@example.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return (body.GetProperty("accessToken").GetString()!, username);
    }

    [Fact]
    public async Task Follow_ValidUser_Returns204()
    {
        var (tokenA, _) = await RegisterAsync("fa");
        var (_, usernameB) = await RegisterAsync("fb");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var resp = await _client.PostAsync($"/api/users/{usernameB}/follow", null);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Unfollow_ExistingFollow_Returns204()
    {
        var (tokenA, _) = await RegisterAsync("ufa");
        var (_, usernameB) = await RegisterAsync("ufb");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        await _client.PostAsync($"/api/users/{usernameB}/follow", null);
        var resp = await _client.DeleteAsync($"/api/users/{usernameB}/follow");

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Like_ValidTweet_Returns204()
    {
        var (tokenA, _) = await RegisterAsync("la");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createResp = await _client.PostAsJsonAsync("/api/tweets", new { text = "like me" });
        createResp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync());
        var tweetId = body.GetProperty("id").GetString();

        var resp = await _client.PostAsync($"/api/tweets/{tweetId}/like", null);

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Unlike_LikedTweet_Returns204()
    {
        var (tokenA, _) = await RegisterAsync("ula");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        var createResp = await _client.PostAsJsonAsync("/api/tweets", new { text = "unlike me" });
        createResp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync());
        var tweetId = body.GetProperty("id").GetString();

        await _client.PostAsync($"/api/tweets/{tweetId}/like", null);
        var resp = await _client.DeleteAsync($"/api/tweets/{tweetId}/like");

        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
