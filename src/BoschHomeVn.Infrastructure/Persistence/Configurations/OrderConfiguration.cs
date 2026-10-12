using BoschHomeVn.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.Code).HasMaxLength(20);
        builder.HasIndex(o => o.Code).IsUnique();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(o => new { o.Status, o.CreatedAt });
        builder.HasIndex(o => o.CreatedAt);

        builder.Property(o => o.CustomerName).HasMaxLength(100);
        builder.Property(o => o.CustomerPhone).HasMaxLength(20);
        builder.Property(o => o.CustomerEmail).HasMaxLength(200);
        builder.Property(o => o.Province).HasMaxLength(100);
        builder.Property(o => o.Ward).HasMaxLength(100);
        builder.Property(o => o.Address).HasMaxLength(500);
        builder.Property(o => o.CustomerNote).HasMaxLength(1000);

        builder.Property(o => o.ShippingMethodId).HasMaxLength(64);
        builder.Property(o => o.ShippingMethodName).HasMaxLength(200);
        builder.Property(o => o.ShippingFee).HasPrecision(18, 0);
        builder.Property(o => o.PaymentMethodId).HasMaxLength(64);
        builder.Property(o => o.PaymentMethodName).HasMaxLength(200);
        builder.Property(o => o.Subtotal).HasPrecision(18, 0);
        builder.Property(o => o.Total).HasPrecision(18, 0);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Events)
            .WithOne()
            .HasForeignKey(e => e.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class OrderEventConfiguration : IEntityTypeConfiguration<OrderEvent>
{
    public void Configure(EntityTypeBuilder<OrderEvent> builder)
    {
        builder.ToTable("OrderEvents");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.OrderId, e.CreatedAt });

        builder.Property(e => e.Actor).HasMaxLength(256);
        builder.Property(e => e.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Note).HasMaxLength(Order.MaxNoteLength);
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        // Không đặt khóa ngoại tới Products: sản phẩm bị xóa thì đơn cũ vẫn còn đủ thông tin
        builder.HasKey(i => new { i.OrderId, i.ProductId });

        builder.Property(i => i.ProductId).HasMaxLength(64);
        builder.Property(i => i.Model).HasMaxLength(32);
        builder.Property(i => i.Name).HasMaxLength(300);
        builder.Property(i => i.ImageUrl).HasMaxLength(500);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 0);
        builder.Ignore(i => i.LineTotal);
    }
}
