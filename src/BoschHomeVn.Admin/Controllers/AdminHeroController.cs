using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Contracts.Home;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/hero")]
public sealed class AdminHeroController(AdminApiClient api) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<HomeHeroResponse>> Get(CancellationToken cancellationToken) =>
        api.GetAsync<HomeHeroResponse>("api/admin/hero", cancellationToken);

    [HttpPut]
    public Task<ActionResult<HomeHeroResponse>> Save(SaveHomeHeroRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<HomeHeroResponse>("api/admin/hero", request, cancellationToken);
}
