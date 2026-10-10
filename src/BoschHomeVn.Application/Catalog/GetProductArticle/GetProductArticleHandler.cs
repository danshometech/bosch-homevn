using BoschHomeVn.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Catalog.GetProductArticle;

public sealed class GetProductArticleHandler(IAppDbContext db)
{
    public Task<string?> Handle(string id, CancellationToken cancellationToken) =>
        (from a in db.ProductArticles.AsNoTracking()
         join p in db.Products on a.ProductId equals p.Id
         where a.ProductId == id && p.Price != null && p.IsPublished
         select a.Html)
            .FirstOrDefaultAsync(cancellationToken);
}
