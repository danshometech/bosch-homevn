using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Home;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace BoschHomeVn.Infrastructure.Persistence;

// Kế thừa IdentityDbContext: các bảng AspNetUsers / AspNetRoles… cho tài khoản trang quản trị
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options), IAppDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<HomeMoment> HomeMoments => Set<HomeMoment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Mỗi entity một IEntityTypeConfiguration<T> trong Persistence/Configurations/
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Tìm không dấu: SearchText.Fold(x) → lower(unaccent(x)) (extension unaccent của PostgreSQL)
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasDbFunction(typeof(SearchText).GetMethod(nameof(SearchText.Fold))!)
            .HasTranslation(args => new SqlFunctionExpression(
                "lower",
                [new SqlFunctionExpression("unaccent", args, nullable: true, argumentsPropagateNullability: [true], typeof(string), args[0].TypeMapping)],
                nullable: true,
                argumentsPropagateNullability: [true],
                typeof(string),
                args[0].TypeMapping));
    }
}
