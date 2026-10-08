using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Home;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Abstractions.Persistence;

// Application đọc/ghi dữ liệu qua interface này, Infrastructure cài đặt bằng EF Core.
// Khi thêm feature, khai báo DbSet tại đây.
public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<ProductType> ProductTypes { get; }
    DbSet<Product> Products { get; }
    DbSet<HomeMoment> HomeMoments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
