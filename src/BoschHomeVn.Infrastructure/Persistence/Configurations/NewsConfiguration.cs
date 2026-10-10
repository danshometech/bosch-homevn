using BoschHomeVn.Domain.News;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class NewsCategoryConfiguration : IEntityTypeConfiguration<NewsCategory>
{
    public void Configure(EntityTypeBuilder<NewsCategory> builder)
    {
        builder.ToTable("NewsCategories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasMaxLength(64);
        builder.Property(c => c.Name).HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(300);
    }
}

internal sealed class NewsPostConfiguration : IEntityTypeConfiguration<NewsPost>
{
    public void Configure(EntityTypeBuilder<NewsPost> builder)
    {
        builder.ToTable("NewsPosts");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CategoryId).HasMaxLength(64);
        builder.HasOne<NewsCategory>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.Title).HasMaxLength(300);
        builder.Property(p => p.Slug).HasMaxLength(200);
        builder.HasIndex(p => p.Slug).IsUnique();
        builder.Property(p => p.Summary).HasMaxLength(500);
        builder.Property(p => p.CoverImageUrl).HasMaxLength(500);
        builder.HasIndex(p => new { p.IsPublished, p.PublishedAt });
    }
}
