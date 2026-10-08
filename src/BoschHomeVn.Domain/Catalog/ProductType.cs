using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Catalog;

// Loại sản phẩm (Bếp từ, Máy rửa bát…). Id là slug, frontend dựng ?loai= trên URL từ tên loại.
// GroupName chỉ có ở Thiết bị bếp (Thiết bị đun nấu, Thiết bị làm sạch…) — slug của nó là ?nhom= trên URL.
public sealed class ProductType : Entity<string>
{
    private ProductType() { } // EF Core

    internal ProductType(string id, string categoryId, string name, string? groupName, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        CategoryId = categoryId;
        Rename(name, groupName);
        SortOrder = sortOrder;
    }

    public string CategoryId { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? GroupName { get; private set; }
    public int SortOrder { get; private set; }

    public void Rename(string name, string? groupName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        GroupName = string.IsNullOrWhiteSpace(groupName) ? null : groupName.Trim();
    }

    internal void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
