using TwitterClone.Domain.Exceptions;

namespace TwitterClone.Domain.Entities;

public class Tweet
{
    public Guid Id { get; private set; }
    public Guid AuthorId { get; private set; }
    public string Text { get; private set; } = default!;
    public Guid? ParentId { get; private set; }
    public string? ImageUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Tweet() { }

    public static Tweet Create(Guid authorId, string text, Guid? parentId = null, string? imageUrl = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("tweet text cannot be empty");
        }

        if (text.Length > 280)
        {
            throw new DomainException("tweet text cannot exceed 280 characters");
        }

        return new Tweet
        {
            Id = Guid.NewGuid(),
            AuthorId = authorId,
            Text = text.Trim(),
            ParentId = parentId,
            ImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
        };
    }
}
