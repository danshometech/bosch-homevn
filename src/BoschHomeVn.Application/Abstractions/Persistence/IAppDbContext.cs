using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Home;
using BoschHomeVn.Domain.News;
using BoschHomeVn.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Abstractions.Persistence;

public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<ProductType> ProductTypes { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductArticle> ProductArticles { get; }
    DbSet<HomeMoment> HomeMoments { get; }
    DbSet<NewsCategory> NewsCategories { get; }
    DbSet<NewsPost> NewsPosts { get; }
    DbSet<SiteSetting> SiteSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
