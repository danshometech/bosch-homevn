using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/moments")]
public sealed class AdminMomentsController(AdminApiClient api) : ControllerBase
{
    [HttpGet]
    public Task<ActionResult<IReadOnlyList<AdminMomentResponse>>> List(CancellationToken cancellationToken) =>
        api.GetAsync<IReadOnlyList<AdminMomentResponse>>("api/admin/moments", cancellationToken);

    [HttpGet("{id}")]
    public Task<ActionResult<AdminMomentResponse>> Get(string id, CancellationToken cancellationToken) =>
        api.GetAsync<AdminMomentResponse>("api/admin/moments/" + Uri.EscapeDataString(id), cancellationToken);

    [HttpPut("{id}")]
    public Task<ActionResult<AdminMomentResponse>> Update(string id, SaveMomentRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<AdminMomentResponse>("api/admin/moments/" + Uri.EscapeDataString(id), request, cancellationToken);
}
