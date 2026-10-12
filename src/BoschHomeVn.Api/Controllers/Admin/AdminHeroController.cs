using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Api.Mappings;
using BoschHomeVn.Application.Home.Hero;
using BoschHomeVn.Contracts.Admin;
using BoschHomeVn.Contracts.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/hero")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminHeroController(HomeHeroHandler handler) : ControllerBase
{
    // Chưa có banner (DB chưa seed) thì trả form trống để admin nhập
    [HttpGet]
    public async Task<HomeHeroResponse> Get(CancellationToken cancellationToken) =>
        (await handler.GetAsync(cancellationToken))?.ToResponse() ?? new HomeHeroResponse("", null, "", null, null, null, "", null, null);

    [HttpPut]
    public async Task<HomeHeroResponse> Save(SaveHomeHeroRequest request, CancellationToken cancellationToken) =>
        (await handler.SaveAsync(request.ToHero(), cancellationToken)).ToResponse();
}
