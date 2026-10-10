using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Checkout;
using BoschHomeVn.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Checkout.Admin;

public sealed record SaveShippingMethodCommand(string Name, string? Description, decimal Fee, bool IsActive);

public sealed record SavePaymentMethodCommand(string Name, string? Description, bool IsInstallment, bool IsActive);

// Trang thanh toán luôn phải còn ít nhất một cách giao hàng và một cách thanh toán không phải trả góp đang bật
public sealed class CheckoutAdminHandler(IAppDbContext db)
{
    private const int MaxIdLength = 60;
    private const string NeedShipping = "Cần ít nhất một cách giao hàng đang bật.";
    private const string NeedPayment = "Cần ít nhất một cách thanh toán (không tính trả góp) đang bật.";

    public async Task<IReadOnlyList<ShippingMethod>> ListShippingAsync(CancellationToken cancellationToken) =>
        await db.ShippingMethods.AsNoTracking().OrderBy(m => m.SortOrder).ToListAsync(cancellationToken);

    public async Task<string> CreateShippingAsync(SaveShippingMethodCommand command, CancellationToken cancellationToken)
    {
        var id = NewId(command.Name, await db.ShippingMethods.Select(m => m.Id).ToListAsync(cancellationToken));
        var last = await db.ShippingMethods.MaxAsync(m => (int?)m.SortOrder, cancellationToken) ?? -1;
        db.ShippingMethods.Add(new ShippingMethod(id, command.Name, command.Description, command.Fee, command.IsActive, last + 1));
        await db.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<bool> UpdateShippingAsync(string id, SaveShippingMethodCommand command, CancellationToken cancellationToken)
    {
        var method = await db.ShippingMethods.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (method is null)
        {
            return false;
        }
        if (!command.IsActive && !await db.ShippingMethods.AnyAsync(m => m.Id != id && m.IsActive, cancellationToken))
        {
            throw new DomainException(NeedShipping);
        }

        method.Update(command.Name, command.Description, command.Fee, command.IsActive);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteShippingAsync(string id, CancellationToken cancellationToken)
    {
        var method = await db.ShippingMethods.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (method is null)
        {
            return false;
        }
        if (method.IsActive && !await db.ShippingMethods.AnyAsync(m => m.Id != id && m.IsActive, cancellationToken))
        {
            throw new DomainException(NeedShipping);
        }

        db.ShippingMethods.Remove(method);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ReorderShippingAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var position = Positions(ids);
        var methods = await db.ShippingMethods.ToListAsync(cancellationToken);
        var ordered = methods.OrderBy(m => position.GetValueOrDefault(m.Id, int.MaxValue)).ThenBy(m => m.SortOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetSortOrder(i);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PaymentMethod>> ListPaymentAsync(CancellationToken cancellationToken) =>
        await db.PaymentMethods.AsNoTracking().OrderBy(m => m.SortOrder).ToListAsync(cancellationToken);

    public async Task<string> CreatePaymentAsync(SavePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        var id = NewId(command.Name, await db.PaymentMethods.Select(m => m.Id).ToListAsync(cancellationToken));
        var last = await db.PaymentMethods.MaxAsync(m => (int?)m.SortOrder, cancellationToken) ?? -1;
        db.PaymentMethods.Add(new PaymentMethod(id, command.Name, command.Description, command.IsInstallment, command.IsActive, last + 1));
        await db.SaveChangesAsync(cancellationToken);
        return id;
    }

    public async Task<bool> UpdatePaymentAsync(string id, SavePaymentMethodCommand command, CancellationToken cancellationToken)
    {
        var method = await db.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (method is null)
        {
            return false;
        }
        if ((!command.IsActive || command.IsInstallment)
            && !await db.PaymentMethods.AnyAsync(m => m.Id != id && m.IsActive && !m.IsInstallment, cancellationToken))
        {
            throw new DomainException(NeedPayment);
        }

        method.Update(command.Name, command.Description, command.IsInstallment, command.IsActive);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeletePaymentAsync(string id, CancellationToken cancellationToken)
    {
        var method = await db.PaymentMethods.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        if (method is null)
        {
            return false;
        }
        if (method.IsActive && !method.IsInstallment
            && !await db.PaymentMethods.AnyAsync(m => m.Id != id && m.IsActive && !m.IsInstallment, cancellationToken))
        {
            throw new DomainException(NeedPayment);
        }

        db.PaymentMethods.Remove(method);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ReorderPaymentAsync(IReadOnlyList<string> ids, CancellationToken cancellationToken)
    {
        var position = Positions(ids);
        var methods = await db.PaymentMethods.ToListAsync(cancellationToken);
        var ordered = methods.OrderBy(m => position.GetValueOrDefault(m.Id, int.MaxValue)).ThenBy(m => m.SortOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            ordered[i].SetSortOrder(i);
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, int> Positions(IReadOnlyList<string> ids) =>
        ids.Distinct().Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);

    private static string NewId(string name, IReadOnlyCollection<string> taken)
    {
        var id = Slug.From(name ?? "");
        if (id.Length > MaxIdLength)
        {
            id = id[..MaxIdLength].TrimEnd('-');
        }
        if (id.Length == 0)
        {
            throw new DomainException("Tên phải có ít nhất một chữ hoặc số.");
        }
        if (!taken.Contains(id))
        {
            return id;
        }

        for (var i = 2; ; i++)
        {
            var next = $"{id}-{i}";
            if (!taken.Contains(next))
            {
                return next;
            }
        }
    }
}
