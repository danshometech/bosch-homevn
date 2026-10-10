using BoschHomeVn.Application.Abstractions.Address;
using BoschHomeVn.Contracts.Address;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

// Danh mục địa chỉ cho form thanh toán. Nguồn ngoài lỗi / quá thời gian thì trả 502 để site chuyển sang nhập tay
[ApiController]
[Route("api/address")]
public sealed class AddressController(IAddressDirectory directory, ILogger<AddressController> logger) : ControllerBase
{
    [HttpGet("provinces")]
    public async Task<ActionResult<IReadOnlyList<AdministrativeUnitResponse>>> GetProvinces(CancellationToken cancellationToken)
    {
        try
        {
            return ToResponse(await directory.GetProvincesAsync(cancellationToken));
        }
        catch (Exception e) when (IsUpstream(e, cancellationToken))
        {
            return Unavailable(e);
        }
    }

    [HttpGet("provinces/{code:int}/wards")]
    public async Task<ActionResult<IReadOnlyList<AdministrativeUnitResponse>>> GetWards(int code, CancellationToken cancellationToken)
    {
        try
        {
            var wards = await directory.GetWardsAsync(code, cancellationToken);
            return wards is null ? NotFound() : ToResponse(wards);
        }
        catch (Exception e) when (IsUpstream(e, cancellationToken))
        {
            return Unavailable(e);
        }
    }

    private static List<AdministrativeUnitResponse> ToResponse(IReadOnlyList<AdministrativeUnit> units) =>
        [.. units.Select(u => new AdministrativeUnitResponse(u.Code, u.Name))];

    private static bool IsUpstream(Exception e, CancellationToken cancellationToken) =>
        !cancellationToken.IsCancellationRequested && e is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException;

    private ObjectResult Unavailable(Exception e)
    {
        logger.LogWarning(e, "Không lấy được danh mục địa chỉ");
        return Problem("Không tải được danh mục địa chỉ, vui lòng thử lại sau.", statusCode: StatusCodes.Status502BadGateway);
    }
}
