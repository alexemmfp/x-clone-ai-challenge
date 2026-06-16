#pragma warning disable CA1716
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Configurations;

internal sealed class LikeConfiguration : IEntityTypeConfiguration<Like>
{
    public void Configure(EntityTypeBuilder<Like> builder)
    {
        builder.HasKey(l => new { l.UserId, l.TweetId });

        builder.Property(l => l.CreatedAt).IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tweet>()
            .WithMany()
            .HasForeignKey(l => l.TweetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("likes");
    }
}
#pragma warning restore CA1716
