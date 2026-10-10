using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Home;
using BoschHomeVn.Domain.News;
using BoschHomeVn.Domain.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace BoschHomeVn.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<IdentityUser>(options), IAppDbContext
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductType> ProductTypes => Set<ProductType>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductArticle> ProductArticles => Set<ProductArticle>();
    public DbSet<HomeMoment> HomeMoments => Set<HomeMoment>();
    public DbSet<NewsCategory> NewsCategories => Set<NewsCategory>();
    public DbSet<NewsPost> NewsPosts => Set<NewsPost>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

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
