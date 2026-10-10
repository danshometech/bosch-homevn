namespace BoschHomeVn.Contracts.Checkout;

public sealed record ShippingMethodResponse(string Id, string Name, string? Description, decimal Fee);

public sealed record PaymentMethodResponse(string Id, string Name, string? Description, bool IsInstallment);

public sealed record CheckoutOptionsResponse(IReadOnlyList<ShippingMethodResponse> Shipping, IReadOnlyList<PaymentMethodResponse> Payment);
