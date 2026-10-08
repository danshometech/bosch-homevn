using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Home.Admin;
using BoschHomeVn.Contracts.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/moments")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminMomentsController(MomentAdminHandler handler) : ControllerBase
{
    [HttpGet]
    public async Task<IReadOnlyList<AdminMomentResponse>> List(CancellationToken cancellationToken) =>
        [.. (await handler.ListAsync(cancellationToken)).Select(m => m.ToAdminResponse())];

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminMomentResponse>> Get(string id, CancellationToken cancellationToken)
    {
        var moment = await handler.GetAsync(id, cancellationToken);
        return moment is null ? NotFound() : moment.ToAdminResponse();
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminMomentResponse>> Update(string id, SaveMomentRequest request, CancellationToken cancellationToken)
    {
        if (!await handler.UpdateAsync(id, request.ToCommand(), cancellationToken))
        {
            return NotFound();
        }
        return (await handler.GetAsync(id, cancellationToken))!.ToAdminResponse();
    }
}
