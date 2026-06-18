using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Tweets;

public class TweetEndpointsTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> RegisterAndGetTokenAsync(string suffix)
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"tweeter{suffix}",
            email = $"tweeter{suffix}@example.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return body.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task CreateTweet_ValidRequest_Returns201()
    {
        var token = await RegisterAndGetTokenAsync("a");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.PostAsJsonAsync("/api/tweets", new { text = "Hello integration!" });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetProperty("text").GetString().Should().Be("Hello integration!");
    }

    [Fact]
    public async Task CreateTweet_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/tweets", new { text = "no auth" });
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTweet_OwnTweet_Returns204()
    {
        var token = await RegisterAndGetTokenAsync("b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResp = await _client.PostAsJsonAsync("/api/tweets", new { text = "to delete" });
        createResp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await createResp.Content.ReadAsStringAsync());
        var id = body.GetProperty("id").GetString();

        var delResp = await _client.DeleteAsync($"/api/tweets/{id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetTimeline_Authenticated_Returns200()
    {
        var token = await RegisterAndGetTokenAsync("c");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.GetAsync("/api/timeline");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
