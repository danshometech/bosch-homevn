namespace BoschHomeVn.Application.Abstractions.Address;

public sealed record AdministrativeUnit(int Code, string Name);

// Danh mục hành chính VN sau sắp xếp 2025: 34 tỉnh / thành → phường / xã (không còn cấp huyện)
public interface IAddressDirectory
{
    Task<IReadOnlyList<AdministrativeUnit>> GetProvincesAsync(CancellationToken cancellationToken);

    // null nếu không có tỉnh / thành mã này
    Task<IReadOnlyList<AdministrativeUnit>?> GetWardsAsync(int provinceCode, CancellationToken cancellationToken);
}
