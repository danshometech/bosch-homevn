using System.Globalization;
using System.Text.RegularExpressions;
using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Application.Common;
using BoschHomeVn.Domain.Catalog;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Orders;

public sealed record PlaceOrderLine(string ProductId, int Quantity, decimal Price);

public sealed record PlaceOrderCommand(
    OrderContact Contact,
    IReadOnlyList<PlaceOrderLine> Lines,
    string ShippingMethodId,
    string PaymentMethodId,
    int? InstallmentMonths,
    decimal Total);

public sealed record PlacedOrder(string Code, decimal Total);

// Khách đặt hàng: kiểm tra lại sản phẩm (còn bán, còn hàng), cách giao / thanh toán còn áp dụng. Giá lưu vào đơn là giá
// khách thấy lúc bấm đặt — phải khớp giá đang bán (không sửa được giá từ trình duyệt); giá / phí vừa đổi thì báo khách xem lại
public sealed partial class PlaceOrderHandler(IAppDbContext db)
{
    public const int MaxLines = 50;
    public const string PriceChanged = "Giá sản phẩm hoặc phí giao hàng vừa thay đổi. Vui lòng kiểm tra lại đơn hàng rồi bấm Đặt hàng lần nữa.";

    public async Task<PlacedOrder> HandleAsync(PlaceOrderCommand c, CancellationToken cancellationToken)
    {
        var contact = Normalize(c.Contact);
        if (c.Lines.Count == 0)
        {
            throw new DomainException("Giỏ hàng đang trống.");
        }
        if (c.Lines.Count > MaxLines)
        {
            throw new DomainException($"Mỗi đơn tối đa {MaxLines} sản phẩm.");
        }
        if (c.Lines.GroupBy(l => l.ProductId).Any(g => g.Count() > 1))
        {
            throw new DomainException("Sản phẩm bị lặp trong đơn hàng.");
        }

        var ids = c.Lines.Select(l => l.ProductId).ToList();
        var products = await db.Products.AsNoTracking().Where(p => ids.Contains(p.Id)).ToDictionaryAsync(p => p.Id, cancellationToken);

        var items = new List<OrderItem>();
        foreach (var line in c.Lines)
        {
            if (!products.TryGetValue(line.ProductId, out var p) || p.Price is not { } price || !p.IsPublished)
            {
                throw new DomainException("Có sản phẩm trong giỏ không còn bán, vui lòng tải lại giỏ hàng.");
            }
            if (p.StockStatus == StockStatus.OutOfStock)
            {
                throw new DomainException($"{p.Name} đã hết hàng, vui lòng bỏ khỏi giỏ hàng.");
            }
            if (p.StockQuantity is { } stock && line.Quantity > stock)
            {
                throw new DomainException($"{p.Name} chỉ còn {stock} sản phẩm.");
            }
            if (line.Price != price)
            {
                throw new DomainException(PriceChanged);
            }
            items.Add(new OrderItem(p.Id, p.Model, p.Name, p.ImageUrl, price, line.Quantity));
        }

        var shipping = await db.ShippingMethods.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == c.ShippingMethodId && m.IsActive, cancellationToken)
            ?? throw new DomainException("Cách giao hàng đã chọn không còn áp dụng, vui lòng chọn lại.");
        var payment = await db.PaymentMethods.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == c.PaymentMethodId && m.IsActive, cancellationToken)
            ?? throw new DomainException("Cách thanh toán đã chọn không còn áp dụng, vui lòng chọn lại.");

        // Trả góp: mọi sản phẩm trong đơn đều cho trả góp với kỳ hạn khách chọn
        int? months = null;
        if (payment.IsInstallment)
        {
            if (c.InstallmentMonths is not { } m
                || ids.Any(id => !products[id].AllowInstallment || !products[id].InstallmentMonths.Contains(m)))
            {
                throw new DomainException("Kỳ hạn trả góp không áp dụng cho sản phẩm trong giỏ, vui lòng chọn lại.");
            }
            months = m;
        }

        var now = DateTime.UtcNow;
        var order = new Order(
            await NewCodeAsync(now, cancellationToken),
            contact,
            items,
            shipping.Id,
            shipping.Name,
            shipping.Fee,
            payment.Id,
            payment.Name,
            months,
            now);
        if (order.Total != c.Total)
        {
            throw new DomainException(PriceChanged);
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);
        return new PlacedOrder(order.Code, order.Total);
    }

    // BH + ngày đặt (giờ Việt Nam, yyMMdd) + 4 số ngẫu nhiên, vd. BH2610120427
    private async Task<string> NewCodeAsync(DateTime now, CancellationToken cancellationToken)
    {
        var day = VietnamTime.FromUtc(now).ToString("yyMMdd", CultureInfo.InvariantCulture);
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = "BH" + day + Random.Shared.Next(10_000).ToString("D4", CultureInfo.InvariantCulture);
            if (!await db.Orders.AnyAsync(o => o.Code == code, cancellationToken))
            {
                return code;
            }
        }
        throw new InvalidOperationException("Không tạo được mã đơn hàng.");
    }

    private static OrderContact Normalize(OrderContact c)
    {
        var name = c.Name?.Trim() ?? "";
        if (name.Length == 0)
        {
            throw new DomainException("Vui lòng nhập họ tên.");
        }

        var phone = PhoneSeparators().Replace(c.Phone ?? "", "");
        if (!Phone().IsMatch(phone))
        {
            throw new DomainException("Số điện thoại gồm 10 số, bắt đầu bằng 0.");
        }

        var email = Clean(c.Email);
        if (email is not null && !Email().IsMatch(email))
        {
            throw new DomainException("Email không hợp lệ.");
        }

        var address = c.Address?.Trim() ?? "";
        if (address.Length == 0)
        {
            throw new DomainException("Vui lòng nhập địa chỉ nhận hàng.");
        }

        return new OrderContact(name, phone, email, Clean(c.Province), Clean(c.Ward), address, Clean(c.Note));
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex(@"[\s.\-]")]
    private static partial Regex PhoneSeparators();

    [GeneratedRegex(@"^0\d{9}$")]
    private static partial Regex Phone();

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex Email();
}
