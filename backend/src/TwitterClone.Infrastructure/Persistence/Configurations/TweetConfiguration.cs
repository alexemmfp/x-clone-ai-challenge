using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TwitterClone.Domain.Entities;

namespace TwitterClone.Infrastructure.Persistence.Configurations;

internal sealed class TweetConfiguration : IEntityTypeConfiguration<Tweet>
{
    public void Configure(EntityTypeBuilder<Tweet> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Text)
            .HasMaxLength(280)
            .IsRequired();

        builder.Property(t => t.ImageUrl)
            .HasMaxLength(512);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Tweet>()
            .WithMany()
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasIndex(t => t.AuthorId);
        builder.HasIndex(t => t.CreatedAt);

        builder.ToTable("tweets");
    }
}
