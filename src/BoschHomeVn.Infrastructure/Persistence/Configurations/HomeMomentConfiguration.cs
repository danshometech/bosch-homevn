using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Home;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class HomeMomentConfiguration : IEntityTypeConfiguration<HomeMoment>
{
    public void Configure(EntityTypeBuilder<HomeMoment> builder)
    {
        builder.ToTable("HomeMoments");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasMaxLength(64);
        builder.Property(m => m.Time).HasMaxLength(5);
        builder.Property(m => m.Name).HasMaxLength(100);
        builder.Property(m => m.Tone).HasMaxLength(20);
        builder.Property(m => m.Title).HasMaxLength(200);
        builder.Property(m => m.Lead).HasMaxLength(1000);
        builder.Property(m => m.ImageUrl).HasMaxLength(500);
        builder.Property(m => m.ImagePosition).HasMaxLength(50);
        builder.Property(m => m.StillImageUrl).HasMaxLength(500);
        builder.Property(m => m.ImageAlt).HasMaxLength(300);
        // Không đặt khóa ngoại: sản phẩm bị xóa thì số liệu tự ẩn, khoảnh khắc vẫn còn
        builder.Property(m => m.FactProductId).HasMaxLength(64);
        builder.Property(m => m.FactSpecKey).HasMaxLength(100);
        builder.Property(m => m.FactNote).HasMaxLength(300);
        builder.Property(m => m.ExtrasTitle).HasMaxLength(200);
        builder.Property(m => m.ExtrasMoreLabel).HasMaxLength(200);
        builder.Property(m => m.ExtrasMoreUrl).HasMaxLength(500);

        builder.ComplexCollection(m => m.Links, links => links.ToJson());

        builder.HasMany(m => m.Products)
            .WithOne()
            .HasForeignKey(p => p.MomentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class HomeMomentProductConfiguration : IEntityTypeConfiguration<HomeMomentProduct>
{
    public void Configure(EntityTypeBuilder<HomeMomentProduct> builder)
    {
        builder.ToTable("HomeMomentProducts");
        builder.HasKey(p => new { p.MomentId, p.ProductId });

        builder.Property(p => p.MomentId).HasMaxLength(64);
        builder.Property(p => p.ProductId).HasMaxLength(64);
        builder.Property(p => p.Slot).HasConversion<string>().HasMaxLength(10);

        // Xóa sản phẩm thì gỡ luôn khỏi các khoảnh khắc
        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(p => p.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
