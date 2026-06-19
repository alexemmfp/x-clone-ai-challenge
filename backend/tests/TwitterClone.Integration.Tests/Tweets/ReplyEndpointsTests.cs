using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Tweets;

public class ReplyEndpointsTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<(string token, string userId)> RegisterAndGetTokenAsync(string suffix)
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"replier{suffix}",
            email = $"replier{suffix}@example.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        var token = body.GetProperty("accessToken").GetString()!;
        var userId = body.GetProperty("userId").GetString()!;
        return (token, userId);
    }

    private async Task<JsonElement> CreateTweetAsync(string token, string text, string? parentId = null)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var payload = parentId is null
            ? (object)new { text }
            : new { text, parentId };
        var resp = await _client.PostAsJsonAsync("/api/tweets", payload);
        resp.EnsureSuccessStatusCode();
        return JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetTweet_ExistingId_Returns200WithTweet()
    {
        var (token, _) = await RegisterAndGetTokenAsync("gt1");
        var tweet = await CreateTweetAsync(token, "Original tweet for get");

        var id = tweet.GetProperty("id").GetString();
        var resp = await _client.GetAsync($"/api/tweets/{id}");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetProperty("id").GetString().Should().Be(id);
        body.GetProperty("text").GetString().Should().Be("Original tweet for get");
    }

    [Fact]
    public async Task GetTweet_NonExistentId_Returns404()
    {
        var (token, _) = await RegisterAndGetTokenAsync("gt2");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.GetAsync($"/api/tweets/{Guid.NewGuid()}");

        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetReplies_AfterPostingReply_ReturnsReply()
    {
        var (token, _) = await RegisterAndGetTokenAsync("rp1");
        var parent = await CreateTweetAsync(token, "Parent tweet");
        var parentId = parent.GetProperty("id").GetString();

        await CreateTweetAsync(token, "First reply", parentId);
        await CreateTweetAsync(token, "Second reply", parentId);

        var resp = await _client.GetAsync($"/api/tweets/{parentId}/replies");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var replies = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        replies.GetArrayLength().Should().Be(2);
        replies[0].GetProperty("parentId").GetString().Should().Be(parentId);
    }

    [Fact]
    public async Task GetReplies_NoReplies_ReturnsEmptyArray()
    {
        var (token, _) = await RegisterAndGetTokenAsync("rp2");
        var tweet = await CreateTweetAsync(token, "Tweet with no replies");
        var id = tweet.GetProperty("id").GetString();

        var resp = await _client.GetAsync($"/api/tweets/{id}/replies");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var replies = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        replies.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task CreateReply_WithParentId_SetsParentId()
    {
        var (token, _) = await RegisterAndGetTokenAsync("rp3");
        var parent = await CreateTweetAsync(token, "Parent");
        var parentId = parent.GetProperty("id").GetString();

        var reply = await CreateTweetAsync(token, "A reply!", parentId);

        reply.GetProperty("parentId").GetString().Should().Be(parentId);
    }
}
