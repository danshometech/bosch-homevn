using BoschHomeVn.Contracts.Address;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

[ApiController]
[Route("api/address")]
public sealed class AddressController(StoreApiClient api) : ControllerBase
{
    [HttpGet("provinces")]
    [ResponseCache(Duration = 3600)]
    public Task<ActionResult<IReadOnlyList<AdministrativeUnitResponse>>> GetProvinces(CancellationToken cancellationToken) =>
        api.GetProvincesAsync(cancellationToken);

    [HttpGet("provinces/{code:int}/wards")]
    [ResponseCache(Duration = 3600)]
    public Task<ActionResult<IReadOnlyList<AdministrativeUnitResponse>>> GetWards(int code, CancellationToken cancellationToken) =>
        api.GetWardsAsync(code, cancellationToken);
}
