using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Checkout;

// IsInstallment: chỉ hiện khi giỏ có sản phẩm cho trả góp, khách chọn số tháng
public sealed class PaymentMethod : Entity<string>
{
    private PaymentMethod() { } // EF Core

    public PaymentMethod(string id, string name, string? description, bool isInstallment, bool isActive, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        Id = id;
        Update(name, description, isInstallment, isActive);
        SortOrder = sortOrder;
    }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsInstallment { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }

    public void Update(string name, string? description, bool isInstallment, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsInstallment = isInstallment;
        IsActive = isActive;
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;
}
