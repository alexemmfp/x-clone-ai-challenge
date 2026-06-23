using FluentAssertions;
using NSubstitute;
using TwitterClone.Application.Interfaces;
using TwitterClone.Application.Media.Commands;

namespace TwitterClone.Application.Tests.Media;

public class UploadImageHandlerTests
{
    private readonly IFileStorageService _storage = Substitute.For<IFileStorageService>();

    [Fact]
    public async Task HandleAsync_ValidFile_ReturnsUrl()
    {
        _storage.SaveAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("/uploads/abc123.jpg");

        var handler = new UploadImageHandler(_storage);
        using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xD9]);

        var result = await handler.HandleAsync(new UploadImageCommand(stream, "photo.jpg", "image/jpeg"));

        result.Should().Be("/uploads/abc123.jpg");
        await _storage.Received(1).SaveAsync(stream, "image/jpeg", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_FileTooLarge_ThrowsInvalidOperationException()
    {
        var handler = new UploadImageHandler(_storage);
        using var stream = new MemoryStream(new byte[6 * 1024 * 1024]);

        var act = () => handler.HandleAsync(new UploadImageCommand(stream, "big.jpg", "image/jpeg"));

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*5 MB*");
    }
}
