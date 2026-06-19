using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace TwitterClone.Integration.Tests.Media;

public class MediaEndpointsTests(TestWebApplicationFactory factory)
    : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private async Task<string> RegisterAndGetTokenAsync(string suffix)
    {
        var resp = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            username = $"uploader{suffix}",
            email = $"uploader{suffix}@example.com",
            password = "Password123!",
        });
        resp.EnsureSuccessStatusCode();
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        return body.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task UploadImage_ValidJpeg_Returns200WithUrl()
    {
        var token = await RegisterAndGetTokenAsync("a");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var imageBytes = CreateMinimalJpeg();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "test.jpg");

        var resp = await _client.PostAsync("/api/media/upload", content);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.TryGetProperty("url", out var urlProp).Should().BeTrue();
        urlProp.GetString().Should().StartWith("/uploads/");
    }

    [Fact]
    public async Task UploadImage_Unauthenticated_Returns401()
    {
        var client = factory.CreateClient();
        using var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent([1, 2, 3]), "file", "test.jpg");

        var resp = await client.PostAsync("/api/media/upload", content);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadImage_TooLarge_Returns400()
    {
        var token = await RegisterAndGetTokenAsync("b");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var content = new MultipartFormDataContent();
        var largeBytes = new byte[6 * 1024 * 1024]; // 6 MB > 5 MB limit
        var fileContent = new ByteArrayContent(largeBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "file", "big.jpg");

        var resp = await _client.PostAsync("/api/media/upload", content);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTweet_WithImageUrl_StoresImageUrl()
    {
        var token = await RegisterAndGetTokenAsync("c");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var resp = await _client.PostAsJsonAsync("/api/tweets", new
        {
            text = "Tweet with image",
            imageUrl = "/uploads/fake-image.jpg",
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = JsonSerializer.Deserialize<JsonElement>(await resp.Content.ReadAsStringAsync());
        body.GetProperty("imageUrl").GetString().Should().Be("/uploads/fake-image.jpg");
    }

    private static byte[] CreateMinimalJpeg()
    {
        // Minimal valid JPEG bytes (SOI + EOI markers)
        return [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
                0x01, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0xFF, 0xD9];
    }
}
