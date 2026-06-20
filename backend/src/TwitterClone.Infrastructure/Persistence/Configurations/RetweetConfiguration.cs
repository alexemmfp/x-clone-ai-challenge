using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Configurations;

internal sealed class RetweetConfiguration : IEntityTypeConfiguration<Retweet>
{
    public void Configure(EntityTypeBuilder<Retweet> builder)
    {
        builder.ToTable("Retweets");
        builder.HasKey(r => r.Id);
        builder.HasIndex(r => new { r.RetweeterId, r.TweetId }).IsUnique();
        builder.HasIndex(r => r.TweetId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.RetweeterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tweet>()
            .WithMany()
            .HasForeignKey(r => r.TweetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
