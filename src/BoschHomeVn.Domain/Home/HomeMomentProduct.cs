namespace BoschHomeVn.Domain.Home;

// Sản phẩm gắn với khoảnh khắc: hàng thẻ chính (Main) hoặc dải "Mua kèm" (Extra), theo SortOrder trong từng nhóm
public sealed class HomeMomentProduct
{
    private HomeMomentProduct() { } // EF Core

    internal HomeMomentProduct(string momentId, string productId, MomentSlot slot, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);

        MomentId = momentId;
        ProductId = productId;
        Slot = slot;
        SortOrder = sortOrder;
    }

    public string MomentId { get; private set; } = null!;
    public string ProductId { get; private set; } = null!;
    public MomentSlot Slot { get; private set; }
    public int SortOrder { get; private set; }

    internal void MoveTo(MomentSlot slot, int sortOrder)
    {
        Slot = slot;
        SortOrder = sortOrder;
    }
}
