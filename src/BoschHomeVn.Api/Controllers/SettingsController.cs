using BoschHomeVn.Application.Settings;
using BoschHomeVn.Contracts.Settings;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("api/settings")]
public sealed class SettingsController(SiteSettingsHandler handler) : ControllerBase
{
    [HttpGet("contact")]
    public async Task<ContactSettingsResponse> GetContact(CancellationToken cancellationToken)
    {
        var s = await handler.GetContactAsync(cancellationToken);
        return new ContactSettingsResponse(s.Hotline, s.ZaloUrl);
    }
}
