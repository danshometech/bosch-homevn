using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Application.Common;
using BoschHomeVn.Domain.Common;
using BoschHomeVn.Domain.Orders;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Orders.Admin;

// Search: mã đơn, số điện thoại (từ 3 chữ số) hoặc tên khách (không dấu). From / To: ngày đặt theo giờ Việt Nam, tính cả hai đầu
public sealed record OrderListQuery(OrderStatus? Status, string? Search, DateOnly? From, DateOnly? To, int Page, int PageSize);

// Counts: số đơn theo từng trạng thái (đã áp dụng Search / khoảng ngày, chưa lọc Status) để hiện trên các tab
public sealed record OrderPage(IReadOnlyList<Order> Items, int Total, IReadOnlyDictionary<OrderStatus, int> Counts);

public sealed class OrderAdminHandler(IAppDbContext db)
{
    // Xuất CSV mỗi lần tối đa chừng này ngày (một tháng dài nhất)
    public const int MaxExportDays = 31;

    public async Task<OrderPage> ListAsync(OrderListQuery q, CancellationToken cancellationToken)
    {
        var query = Filter(db.Orders.AsNoTracking(), q.Search, q.From, q.To);
        var counts = await query
            .GroupBy(o => o.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
        if (q.Status is { } status)
        {
            query = query.Where(o => o.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(cancellationToken);
        return new OrderPage(items, total, counts);
    }

    // Xuất CSV: mọi đơn đặt trong khoảng ngày (tối đa MaxExportDays ngày), mới nhất trước, kèm sản phẩm
    public async Task<IReadOnlyList<Order>> ExportAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (from > to)
        {
            throw new DomainException("Ngày bắt đầu phải trước ngày kết thúc.");
        }
        if (to.DayNumber - from.DayNumber + 1 > MaxExportDays)
        {
            throw new DomainException($"Mỗi lần xuất tối đa {MaxExportDays} ngày.");
        }

        return await Filter(db.Orders.AsNoTracking(), null, from, to)
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    private static IQueryable<Order> Filter(IQueryable<Order> query, string? search, DateOnly? from, DateOnly? to)
    {
        if (from is { } f)
        {
            var start = VietnamTime.StartOfDayUtc(f);
            query = query.Where(o => o.CreatedAt >= start);
        }
        if (to is { } t)
        {
            var end = VietnamTime.StartOfDayUtc(t.AddDays(1));
            query = query.Where(o => o.CreatedAt < end);
        }
        if (string.IsNullOrWhiteSpace(search))
        {
            return query;
        }

        var code = search.Trim().ToUpperInvariant();
        var digits = new string(search.Where(char.IsDigit).ToArray());
        var name = SearchText.Normalize(search);
        return query.Where(o =>
            o.Code.Contains(code)
            || (digits.Length >= 3 && o.CustomerPhone.Contains(digits))
            || SearchText.Fold(o.CustomerName).Contains(name));
    }

    public Task<int> CountNewAsync(CancellationToken cancellationToken) =>
        db.Orders.CountAsync(o => o.Status == OrderStatus.New, cancellationToken);

    // Kèm sản phẩm và lịch sử xử lý (trang chi tiết, xuất chi tiết một đơn)
    public async Task<Order?> GetAsync(string code, CancellationToken cancellationToken) =>
        await db.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Events)
            .FirstOrDefaultAsync(o => o.Code == code, cancellationToken);

    // actor: tài khoản admin đang thao tác (ghi vào lịch sử)
    public async Task<bool> UpdateAsync(string code, OrderStatus status, bool isPaid, string? note, string actor, CancellationToken cancellationToken)
    {
        var order = await db.Orders.Include(o => o.Events).FirstOrDefaultAsync(o => o.Code == code, cancellationToken);
        if (order is null)
        {
            return false;
        }

        order.Update(status, isPaid, note, actor, DateTime.UtcNow);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
