using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Catalog;

// Id = model viết thường ("smv4hcx48e") — trùng id trên URL /san-pham/chi-tiet/:id của site
public sealed class Product : Entity<string>
{
    private Product() { } // EF Core

    public Product(string model, string name, string productTypeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(productTypeId);

        Model = model.Trim().ToUpperInvariant();
        Id = Model.ToLowerInvariant();
        Name = name.Trim();
        ProductTypeId = productTypeId;
        IsPublished = true;
    }

    public string Model { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string ProductTypeId { get; private set; } = null!;
    public int? Series { get; private set; }

    // Giá bán; null = chưa bán trên site (chờ admin nhập giá)
    public decimal? Price { get; private set; }
    // Giá gạch hiển thị cạnh giá bán
    public decimal? OldPrice { get; private set; }
    // Giá nhập đại lý — chỉ dùng nội bộ, không trả ra API công khai
    public decimal? DealerPrice { get; private set; }

    public StockStatus StockStatus { get; private set; }
    public int? StockQuantity { get; private set; }
    // Ghi chú về hàng, vd. "Cuối Tháng 09 có hàng"
    public string? StockNote { get; private set; }

    public string? Color { get; private set; }
    public string? Origin { get; private set; }
    public string? Warranty { get; private set; }

    // Admin tắt thì sản phẩm không hiện trên site dù đã có giá bán
    public bool IsPublished { get; private set; }
    public bool IsNew { get; private set; }
    public bool IsFlashSale { get; private set; }
    public decimal? Rating { get; private set; }
    public int ReviewCount { get; private set; }
    public string? ImageUrl { get; private set; }
    // Ảnh thêm cho trang chi tiết, sau ảnh chính ImageUrl (trang chi tiết hiện tối đa 5 ảnh nhỏ)
    public List<string> GalleryImages { get; private set; } = [];
    public const int MaxGalleryImages = 4;
    // Video giới thiệu (MP4 / WebM đã tải lên), hiện thành ô ▶ trong thư viện ảnh trang chi tiết;
    // ảnh bìa = một khung hình của video (admin chụp lúc tải lên), hiện ở ô ▶ và trong lúc video đang tải
    public string? VideoUrl { get; private set; }
    public string? VideoPosterUrl { get; private set; }

    // 3 thông số nổi bật trên thẻ sản phẩm
    public List<string> Highlights { get; private set; } = [];
    // Bảng thông số kỹ thuật, giữ đúng thứ tự hiển thị
    public List<ProductSpec> Specs { get; private set; } = [];

    // Trả góp 0%: admin bật mới có. InstallmentMonths = các kỳ hạn khách chọn khi thanh toán,
    // InstallmentDisplayMonths = kỳ hạn dùng tính "chỉ X/tháng" trên thẻ / trang chi tiết
    public bool AllowInstallment { get; private set; }
    public List<int> InstallmentMonths { get; private set; } = [];
    public int? InstallmentDisplayMonths { get; private set; }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
    }

    public void ChangeType(string productTypeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productTypeId);
        ProductTypeId = productTypeId;
    }

    public void SetPublished(bool isPublished) => IsPublished = isPublished;

    public void SetGallery(IEnumerable<string> urls)
    {
        var list = urls.Select(u => u.Trim()).Where(u => u.Length > 0).Distinct().ToList();
        if (list.Count > MaxGalleryImages)
        {
            throw new DomainException($"Tối đa {MaxGalleryImages} ảnh thêm.");
        }
        if (list.Any(u => u.Length > 500))
        {
            throw new DomainException("Đường dẫn ảnh tối đa 500 ký tự.");
        }

        GalleryImages = list;
    }

    public void SetVideo(string? url, string? posterUrl)
    {
        url = string.IsNullOrWhiteSpace(url) ? null : url.Trim();
        posterUrl = url is null || string.IsNullOrWhiteSpace(posterUrl) ? null : posterUrl.Trim();
        if (url?.Length > 500 || posterUrl?.Length > 500)
        {
            throw new DomainException("Đường dẫn video / ảnh bìa tối đa 500 ký tự.");
        }

        VideoUrl = url;
        VideoPosterUrl = posterUrl;
    }

    public void SetInstallment(bool allow, IEnumerable<int> months, int? displayMonths)
    {
        var list = months.Distinct().Order().ToList();
        if (list.Any(m => m is < 1 or > 60))
        {
            throw new DomainException("Kỳ hạn trả góp phải từ 1 đến 60 tháng.");
        }
        // Bật mà chưa chọn kỳ hạn (vd. bật nhanh trên bảng) thì dùng 3/6/9/12 tháng, hiển thị theo 12 tháng
        if (allow && list.Count == 0)
        {
            list = [3, 6, 9, 12];
            displayMonths = 12;
        }
        if (list.Count > 0 && (displayMonths is null || !list.Contains(displayMonths.Value)))
        {
            throw new DomainException("Kỳ hạn hiển thị phải là một trong các kỳ hạn trả góp đã chọn.");
        }

        AllowInstallment = allow;
        InstallmentMonths = list;
        InstallmentDisplayMonths = list.Count > 0 ? displayMonths : null;
    }

    public void SetPrice(decimal? price, decimal? oldPrice)
    {
        if (price <= 0)
        {
            throw new DomainException("Giá bán phải lớn hơn 0.");
        }
        if (oldPrice is not null && (price is null || oldPrice < price))
        {
            throw new DomainException("Giá gạch phải lớn hơn hoặc bằng giá bán.");
        }

        Price = price;
        OldPrice = oldPrice;
    }

    public void SetDealerPrice(decimal? dealerPrice)
    {
        if (dealerPrice <= 0)
        {
            throw new DomainException("Giá nhập phải lớn hơn 0.");
        }

        DealerPrice = dealerPrice;
    }

    public void SetStock(StockStatus status, int? quantity, string? note)
    {
        if (quantity < 0)
        {
            throw new DomainException("Số lượng tồn không được âm.");
        }

        StockStatus = status;
        StockQuantity = quantity;
        StockNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public void SetDetails(int? series, string? color, string? origin, string? warranty)
    {
        Series = series;
        Color = color;
        Origin = origin;
        Warranty = warranty;
    }

    public void SetRating(decimal? rating, int reviewCount)
    {
        if (rating is < 0 or > 5)
        {
            throw new DomainException("Điểm đánh giá phải trong khoảng 0–5.");
        }
        if (reviewCount < 0)
        {
            throw new DomainException("Số đánh giá không được âm.");
        }

        Rating = rating;
        ReviewCount = reviewCount;
    }

    public void SetDisplay(bool isNew, bool isFlashSale, string? imageUrl, IEnumerable<string> highlights, IEnumerable<ProductSpec> specs)
    {
        IsNew = isNew;
        IsFlashSale = isFlashSale;
        ImageUrl = imageUrl;
        Highlights = [.. highlights];
        Specs = [.. specs];
    }
}
