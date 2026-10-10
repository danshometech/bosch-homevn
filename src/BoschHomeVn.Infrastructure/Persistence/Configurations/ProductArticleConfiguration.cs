using BoschHomeVn.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class ProductArticleConfiguration : IEntityTypeConfiguration<ProductArticle>
{
    public void Configure(EntityTypeBuilder<ProductArticle> builder)
    {
        builder.ToTable("ProductArticles");
        builder.HasKey(a => a.ProductId);

        builder.Property(a => a.ProductId).HasMaxLength(64);
        builder.HasOne<Product>()
            .WithOne()
            .HasForeignKey<ProductArticle>(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
