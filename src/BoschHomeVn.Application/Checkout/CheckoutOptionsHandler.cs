using BoschHomeVn.Application.Abstractions.Persistence;
using BoschHomeVn.Domain.Checkout;
using Microsoft.EntityFrameworkCore;

namespace BoschHomeVn.Application.Checkout;

public sealed record CheckoutOptions(IReadOnlyList<ShippingMethod> Shipping, IReadOnlyList<PaymentMethod> Payment);

public sealed class CheckoutOptionsHandler(IAppDbContext db)
{
    public async Task<CheckoutOptions> GetAsync(CancellationToken cancellationToken)
    {
        var shipping = await db.ShippingMethods.AsNoTracking().Where(m => m.IsActive).OrderBy(m => m.SortOrder).ToListAsync(cancellationToken);
        var payment = await db.PaymentMethods.AsNoTracking().Where(m => m.IsActive).OrderBy(m => m.SortOrder).ToListAsync(cancellationToken);
        return new CheckoutOptions(shipping, payment);
    }
}
