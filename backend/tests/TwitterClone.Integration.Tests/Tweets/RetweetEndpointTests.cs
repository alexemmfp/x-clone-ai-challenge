using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Tweets;

public class RetweetEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string token, string username, Guid userId)> RegisterAsync(string suffix)
    {
        var username = $"rt{suffix}";
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"rt{suffix}@example.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return (body.GetProperty("accessToken").GetString()!, username,
                Guid.Parse(body.GetProperty("userId").GetString()!));
    }

    private async Task<string> CreateTweetAsync(string token, string text)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var resp = await _client.PostAsJsonAsync("/api/tweets", new { text });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return body.GetProperty("id").GetString()!;
    }

    [Fact]
    public async Task Retweet_OtherUsersTweet_Returns200WithCount()
    {
        var (tokenA, _, _) = await RegisterAsync("ra1");
        var (tokenB, _, _) = await RegisterAsync("ra2");

        var tweetId = await CreateTweetAsync(tokenA, "retweet me");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        var resp = await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetProperty("retweetCount").GetInt32().Should().Be(1);
    }

    [Fact]
    public async Task Retweet_OwnTweet_Returns400()
    {
        var (tokenA, _, _) = await RegisterAsync("rb1");
        var tweetId = await CreateTweetAsync(tokenA, "cant retweet own");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var resp = await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unretweet_ExistingRetweet_Returns204()
    {
        var (tokenA, _, _) = await RegisterAsync("rc1");
        var (tokenB, _, _) = await RegisterAsync("rc2");

        var tweetId = await CreateTweetAsync(tokenA, "unretweet me");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);
        await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        var resp = await _client.DeleteAsync($"/api/tweets/{tweetId}/retweet");
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Timeline_IncludesRetweetsFromFollowedUsers()
    {
        var (tokenA, usernameA, _) = await RegisterAsync("rd1");
        var (tokenB, usernameB, _) = await RegisterAsync("rd2");
        var (tokenC, _, _) = await RegisterAsync("rd3");

        // C follows A; A retweets B's tweet; C's timeline should show the retweet
        var tweetId = await CreateTweetAsync(tokenB, "original tweet from B");

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await _client.PostAsync($"/api/users/{usernameB}/follow", null);
        await _client.PostAsync($"/api/tweets/{tweetId}/retweet", null);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenC);
        await _client.PostAsync($"/api/users/{usernameA}/follow", null);

        var resp = await _client.GetAsync("/api/timeline");
        resp.EnsureSuccessStatusCode();
        var tweets = JsonSerializer.Deserialize<JsonElement[]>(
            await resp.Content.ReadAsStringAsync(),
            JsonOptions)!;

        tweets.Should().Contain(t =>
            t.GetProperty("id").GetString() == tweetId &&
            t.GetProperty("isRetweet").GetBoolean() == true &&
            t.GetProperty("retweetedByUsername").GetString() == usernameA);
    }
}
