using BoschHomeVn.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasMaxLength(64);
        builder.Property(p => p.Model).HasMaxLength(32);
        builder.HasIndex(p => p.Model).IsUnique();
        // Collation không phân biệt dấu (kể cả đ/d) để tìm "may rua bat" ra "Máy rửa bát"
        builder.Property(p => p.Name).HasMaxLength(300).UseCollation(Collations.AccentInsensitive);

        builder.Property(p => p.ProductTypeId).HasMaxLength(64);
        builder.HasOne<ProductType>()
            .WithMany()
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Tiền VND không có phần lẻ
        builder.Property(p => p.Price).HasPrecision(18, 0);
        builder.Property(p => p.OldPrice).HasPrecision(18, 0);
        builder.Property(p => p.DealerPrice).HasPrecision(18, 0);

        builder.Property(p => p.StockStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.StockNote).HasMaxLength(200);

        builder.Property(p => p.Color).HasMaxLength(100);
        builder.Property(p => p.Origin).HasMaxLength(100);
        builder.Property(p => p.Warranty).HasMaxLength(100);
        builder.Property(p => p.Rating).HasPrecision(2, 1);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
        builder.Property(p => p.VideoUrl).HasMaxLength(500);
        builder.Property(p => p.VideoPosterUrl).HasMaxLength(500);

        // Lưu dạng cột JSON
        builder.PrimitiveCollection(p => p.Highlights);
        builder.PrimitiveCollection(p => p.InstallmentMonths);
        builder.PrimitiveCollection(p => p.GalleryImages);
        builder.ComplexCollection(p => p.Specs, specs => specs.ToJson());
    }
}
