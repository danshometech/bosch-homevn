using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Checkout;

public sealed class ShippingMethod : Entity<string>
{
    private ShippingMethod() { } // EF Core

    public ShippingMethod(string id, string name, string? description, decimal fee, bool isActive, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        Update(name, description, fee, isActive);
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Fee { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    public void Update(string name, string? description, decimal fee, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (fee < 0)
        {
            throw new DomainException("Phí giao hàng không được âm.");
        }

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Fee = decimal.Round(fee);
        IsActive = isActive;
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
