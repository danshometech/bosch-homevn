using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.News;

public sealed class NewsCategory : Entity<string>
{
    private NewsCategory() { } // EF Core

    public NewsCategory(string id, string name, string? description, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        Update(name, description);
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }

    public void Update(string name, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
