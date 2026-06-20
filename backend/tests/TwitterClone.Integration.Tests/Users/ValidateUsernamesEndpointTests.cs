using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Users;

public class ValidateUsernamesEndpointTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _client = factory.CreateClient();

    private async Task RegisterAsync(string username)
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username,
            email = $"{username}@test.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ValidateUsernames_ReturnsCorrectMap()
    {
        await RegisterAsync("mentionuserA");
        await RegisterAsync("mentionuserB");

        var resp = await _client.GetAsync(
            "/api/users/validate-usernames?usernames=mentionuserA,mentionuserB,nobody123");

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<Dictionary<string, bool>>(
            await resp.Content.ReadAsStringAsync(),
            JsonOptions)!;

        body["mentionuserA"].Should().BeTrue();
        body["mentionuserB"].Should().BeTrue();
        body["nobody123"].Should().BeFalse();
    }

    [Fact]
    public async Task ValidateUsernames_EmptyParam_ReturnsEmptyObject()
    {
        var resp = await _client.GetAsync("/api/users/validate-usernames");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadAsStringAsync();
        body.Should().Be("{}");
    }
}
