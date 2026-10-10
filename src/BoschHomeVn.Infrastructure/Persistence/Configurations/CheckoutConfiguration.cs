using BoschHomeVn.Domain.Checkout;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class ShippingMethodConfiguration : IEntityTypeConfiguration<ShippingMethod>
{
    public void Configure(EntityTypeBuilder<ShippingMethod> builder)
    {
        builder.ToTable("ShippingMethods");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasMaxLength(64);
        builder.Property(m => m.Name).HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(300);
        builder.Property(m => m.Fee).HasPrecision(18, 0);
    }
}

internal sealed class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("PaymentMethods");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasMaxLength(64);
        builder.Property(m => m.Name).HasMaxLength(200);
        builder.Property(m => m.Description).HasMaxLength(300);
    }
}
