using BoschHomeVn.Contracts.Home;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

[ApiController]
[Route("api/home")]
public sealed class HomePageController(StoreApiClient api) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<HomePageResponse>> Get(CancellationToken cancellationToken) => api.GetHomeAsync(cancellationToken);
}
