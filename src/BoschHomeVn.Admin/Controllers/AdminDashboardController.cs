using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
public sealed class AdminDashboardController(AdminApiClient api) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<DashboardResponse>> Get(CancellationToken cancellationToken) =>
        api.GetAsync<DashboardResponse>("api/admin/dashboard", cancellationToken);
}
