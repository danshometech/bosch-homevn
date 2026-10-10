using System.ComponentModel.DataAnnotations;

namespace BoschHomeVn.Contracts.Admin;

public sealed record AdminShippingMethodResponse(string Id, string Name, string? Description, decimal Fee, bool IsActive);

public sealed record SaveShippingMethodRequest(
    [StringLength(200)] string Name,
    [StringLength(300)] string? Description,
    [Range(0d, 1_000_000_000d)] decimal Fee,
    bool IsActive);

public sealed record AdminPaymentMethodResponse(string Id, string Name, string? Description, bool IsInstallment, bool IsActive);

public sealed record SavePaymentMethodRequest(
    [StringLength(200)] string Name,
    [StringLength(300)] string? Description,
    bool IsInstallment,
    bool IsActive);
