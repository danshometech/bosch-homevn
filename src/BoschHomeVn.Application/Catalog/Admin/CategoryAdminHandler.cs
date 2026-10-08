using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Catalog.Admin;

// ProductCountByType: số sản phẩm (mọi trạng thái) của từng loại — loại còn sản phẩm thì không xóa được
public sealed record CategoryTree(IReadOnlyList<Category> Categories, IReadOnlyDictionary<string, int> ProductCountByType);

// Quản trị danh mục + loại sản phẩm. Id sinh từ tên (slug, giống slugify của frontend) và không đổi khi đổi tên.
public sealed class CategoryAdminHandler(IAppDbContext db)
{
    public async Task<CategoryTree> GetTreeAsync(CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Include(c => c.Types.OrderBy(t => t.SortOrder))
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);
        var counts = await db.Products
            .GroupBy(p => p.ProductTypeId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        return new CategoryTree(categories, counts);
    }

    public async Task<string> CreateCategoryAsync(string name, string shortName, CancellationToken cancellationToken)
    {
        var id = NewId(name);
        if (await db.Categories.AnyAsync(c => c.Id == id, cancellationToken))
        {
            throw new DomainException($"Đã có danh mục mã \"{id}\".");
        }

        var last = await db.Categories.MaxAsync(c => (int?)c.SortOrder, cancellationToken) ?? -1;
        db.Categories.Add(new Category(id, name, shortName, last + 1));
        await db.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<bool> UpdateCategoryAsync(string id, string name, string shortName, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.Rename(name, shortName);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(string id, CancellationToken cancellationToken)
    {
        var category = await db.Categories.Include(c => c.Types).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return false;
        }
        if (category.Types.Count > 0)
        {
            throw new DomainException("Danh mục còn loại sản phẩm — xóa hoặc chuyển hết loại trước.");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ReorderCategoriesAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var position = ids.Distinct().Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        var categories = await db.Categories.ToListAsync(cancellationToken);
        var ordered = categories.OrderBy(c => position.GetValueOrDefault(c.Id, int.MaxValue)).ThenBy(c => c.SortOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetSortOrder(i);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    // null nếu không có danh mục
    public async Task<string?> CreateTypeAsync(string categoryId, string name, string? groupName, CancellationToken cancellationToken)
    {
        var category = await db.Categories.Include(c => c.Types).FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
        if (category is null)
        {
            return null;
        }

        var id = NewId(name);
        if (await db.ProductTypes.AnyAsync(t => t.Id == id, cancellationToken))
        {
            throw new DomainException($"Đã có loại sản phẩm mã \"{id}\".");
        }

        category.AddType(id, name, groupName);
        await db.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<bool> UpdateTypeAsync(string id, string name, string? groupName, CancellationToken cancellationToken)
    {
        var type = await db.ProductTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (type is null)
        {
            return false;
        }

        type.Rename(name, groupName);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteTypeAsync(string id, CancellationToken cancellationToken)
    {
        var type = await db.ProductTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (type is null)
        {
            return false;
        }
        if (await db.Products.AnyAsync(p => p.ProductTypeId == id, cancellationToken))
        {
            throw new DomainException("Loại này còn sản phẩm — chuyển sản phẩm sang loại khác hoặc xóa trước.");
        }

        db.ProductTypes.Remove(type);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ReorderTypesAsync(string categoryId, IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var category = await db.Categories.Include(c => c.Types).FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);
        if (category is null)
        {
            return false;
        }

        category.ReorderTypes(ids);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string NewId(string name)
    {
        var id = Slug.From(name ?? "");
        return id.Length > 0 ? id : throw new DomainException("Tên phải có ít nhất một chữ hoặc số.");
    }
}
