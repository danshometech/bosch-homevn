using BoschHomeVn.Domain.Common;

namespace BoschHomeVn.Domain.Home;

// Một khoảnh khắc trên trang chủ "A Day with Bosch" (06:45 cà phê, 12:30 bữa trưa…): ảnh bối cảnh, lời dẫn,
// hàng sản phẩm chính và dải "Mua kèm". Id là slug, dùng làm anchor trên trang (#bua-trua).
public sealed class HomeMoment : Entity<string>
{
    // Tông màu nền theo giờ — bảng màu tương ứng nằm ở frontend (web/webstore/src/data/day.js, TONES)
    public static readonly IReadOnlyList<string> Tones = ["dawn", "morning", "day", "noon", "dusk", "night"];

    private readonly List<HomeMomentProduct> _products = [];

    private HomeMoment() { } // EF Core

    public HomeMoment(string id, string time, string name, string tone, string title, string lead, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(time);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tone);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Id = id;
        Time = time;
        Name = name;
        Tone = tone;
        Title = title;
        Lead = lead;
        SortOrder = sortOrder;
    }

    // Giờ hiển thị "06:45"
    public string Time { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    // Tông màu nền theo giờ (dawn, morning, day, noon, dusk, night) — bảng màu nằm ở frontend
    public string Tone { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Lead { get; private set; } = null!;
    public int SortOrder { get; private set; }

    // Ảnh bối cảnh (ImageUrl + ImagePosition = object-position) hoặc ảnh tĩnh sản phẩm trên mảng tường (StillImageUrl)
    public string? ImageUrl { get; private set; }
    public string? ImagePosition { get; private set; }
    public string? StillImageUrl { get; private set; }
    public string? ImageAlt { get; private set; }

    // Số liệu đè trên ảnh: giá trị lấy từ Specs[FactSpecKey] của sản phẩm FactProductId, kèm câu chú thích
    public string? FactProductId { get; private set; }
    public string? FactSpecKey { get; private set; }
    public string? FactNote { get; private set; }

    public List<MomentLink> Links { get; private set; } = [];

    public string? ExtrasTitle { get; private set; }
    public string? ExtrasMoreLabel { get; private set; }
    public string? ExtrasMoreUrl { get; private set; }

    public IReadOnlyList<HomeMomentProduct> Products => _products;

    public void Update(string time, string name, string tone, string title, string lead)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(time);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(tone);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        if (!TimeOnly.TryParseExact(time, "HH:mm", out _))
        {
            throw new DomainException("Giờ phải có dạng HH:mm, vd. 06:45.");
        }
        if (!Tones.Contains(tone))
        {
            throw new DomainException($"Tông màu phải là một trong: {string.Join(", ", Tones)}.");
        }

        Time = time;
        Name = name.Trim();
        Tone = tone;
        Title = title.Trim();
        Lead = lead?.Trim() ?? "";
    }

    public void SetScene(string? imageUrl, string? imagePosition, string? stillImageUrl, string? imageAlt)
    {
        if (imageUrl is null && stillImageUrl is null)
        {
            throw new DomainException("Khoảnh khắc cần ảnh bối cảnh hoặc ảnh tĩnh.");
        }

        ImageUrl = imageUrl;
        ImagePosition = imagePosition;
        StillImageUrl = stillImageUrl;
        ImageAlt = imageAlt;
    }

    public void SetFact(string? productId, string? specKey, string? note)
    {
        FactProductId = string.IsNullOrWhiteSpace(productId) ? null : productId;
        FactSpecKey = specKey;
        FactNote = note;
    }

    public void SetLinks(IEnumerable<MomentLink> links) => Links = [.. links];

    public void SetExtras(string? title, string? moreLabel, string? moreUrl)
    {
        ExtrasTitle = title;
        ExtrasMoreLabel = moreLabel;
        ExtrasMoreUrl = moreUrl;
    }

    // Thay toàn bộ danh sách sản phẩm (giữ dòng cũ nếu sản phẩm vẫn còn, chỉ đổi vị trí) — một sản phẩm chỉ ở một chỗ
    public void SetProducts(IReadOnlyList<string> mainIds, IReadOnlyList<string> extraIds)
    {
        var wanted = mainIds.Select((id, i) => (id, slot: MomentSlot.Main, i))
            .Concat(extraIds.Select((id, i) => (id, slot: MomentSlot.Extra, i)))
            .ToList();
        var duplicate = wanted.GroupBy(x => x.id).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new DomainException($"Sản phẩm {duplicate.Key} bị chọn hai lần trong khoảnh khắc.");
        }

        _products.RemoveAll(p => wanted.All(w => w.id != p.ProductId));
        foreach (var (id, slot, i) in wanted)
        {
            var existing = _products.Find(p => p.ProductId == id);
            if (existing is null)
            {
                _products.Add(new HomeMomentProduct(Id, id, slot, i));
            }
            else
            {
                existing.MoveTo(slot, i);
            }
        }
    }

    public void AddProduct(string productId, MomentSlot slot)
    {
        if (_products.Any(p => p.ProductId == productId))
        {
            throw new DomainException($"Sản phẩm {productId} đã có trong khoảnh khắc {Id}.");
        }

        _products.Add(new HomeMomentProduct(Id, productId, slot, _products.Count(p => p.Slot == slot)));
    }
}
