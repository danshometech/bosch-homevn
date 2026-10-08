using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Catalog;

// Danh mục cấp 1 trên menu (Thiết bị bếp, Gia dụng, Khóa cửa vân tay). Id là slug dùng trên URL: /san-pham/thiet-bi-bep
public sealed class Category : Entity<string>
{
    private readonly List<ProductType> _types = [];

    private Category() { } // EF Core

    public Category(string id, string name, string shortName, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        Rename(name, shortName);
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = null!;
    public string ShortName { get; private set; } = null!;
    public int SortOrder { get; private set; }
    public IReadOnlyList<ProductType> Types => _types;

    public void Rename(string name, string shortName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(shortName);

        Name = name.Trim();
        ShortName = shortName.Trim();
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    public ProductType AddType(string id, string name, string? groupName)
    {
        var type = new ProductType(id, Id, name, groupName, _types.Count == 0 ? 0 : _types.Max(t => t.SortOrder) + 1);
        _types.Add(type);
        return type;
    }

    public void RemoveType(ProductType type) => _types.Remove(type);

    // Sắp lại thứ tự loại theo danh sách id (id thiếu thì giữ nguyên, xếp sau)
    public void ReorderTypes(IReadOnlyList<string> ids)
    {
        var position = ids.Distinct().Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
        var ordered = _types.OrderBy(t => position.GetValueOrDefault(t.Id, int.MaxValue)).ThenBy(t => t.SortOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetSortOrder(i);
        }
    }
}
