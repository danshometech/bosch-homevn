using BoschHomeVn.Contracts.Settings;
using BoschHomeVn.WebStore.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController(StoreApiClient api) : ControllerBase
{
    [HttpGet("contact")]
    public Task<ActionResult<ContactSettingsResponse>> GetContact(CancellationToken cancellationToken) =>
        api.GetContactSettingsAsync(cancellationToken);
}
