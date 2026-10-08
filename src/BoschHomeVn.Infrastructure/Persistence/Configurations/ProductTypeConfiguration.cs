using BoschHomeVn.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BoschHomeVn.Infrastructure.Persistence.Configurations;

internal sealed class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("ProductTypes");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id).HasMaxLength(64);
        builder.Property(t => t.CategoryId).HasMaxLength(64);
        builder.Property(t => t.Name).HasMaxLength(200).UseCollation(Collations.AccentInsensitive);
        builder.Property(t => t.GroupName).HasMaxLength(200);
    }
}
