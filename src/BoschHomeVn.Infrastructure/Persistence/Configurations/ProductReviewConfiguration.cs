using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
{
    public void Configure(EntityTypeBuilder<ProductReview> builder)
    {
        builder.ToTable("ProductReviews");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProductId).HasMaxLength(64);
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.Content).HasMaxLength(ProductReview.MaxContentLength);
        builder.Property(r => r.AuthorName).HasMaxLength(ProductReview.MaxNameLength);
        builder.Property(r => r.AuthorPhone).HasMaxLength(20);
        builder.Property(r => r.Reply).HasMaxLength(ProductReview.MaxReplyLength);
        builder.Property(r => r.CreatedBy).HasMaxLength(256);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.PrimitiveCollection(r => r.Photos);

        builder.HasIndex(r => new { r.ProductId, r.Status, r.CreatedAt });
        builder.HasIndex(r => new { r.Status, r.CreatedAt });
    }
}

internal sealed class ReviewVoteConfiguration : IEntityTypeConfiguration<ReviewVote>
{
    public void Configure(EntityTypeBuilder<ReviewVote> builder)
    {
        builder.ToTable("ReviewVotes");
        builder.HasKey(v => new { v.ReviewId, v.VisitorId });
        builder.Property(v => v.VisitorId).HasMaxLength(ReviewVote.MaxVisitorIdLength);
        builder.HasIndex(v => v.VisitorId);
        builder.HasOne<ProductReview>()
            .WithMany()
            .HasForeignKey(v => v.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
