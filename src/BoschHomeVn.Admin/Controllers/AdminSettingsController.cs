using BoschHomeVn.Admin.Infrastructure;
using BoschHomeVn.Contracts.Settings;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

[ApiController]
[Route("api/admin/settings")]
public sealed class AdminSettingsController(AdminApiClient api) : ControllerBase
{
    [HttpGet("contact")]
    public Task<ActionResult<ContactSettingsResponse>> GetContact(CancellationToken cancellationToken) =>
        api.GetAsync<ContactSettingsResponse>("api/admin/settings/contact", cancellationToken);

    [HttpPut("contact")]
    public Task<ActionResult<ContactSettingsResponse>> SaveContact(SaveContactSettingsRequest request, CancellationToken cancellationToken) =>
        api.PutAsync<ContactSettingsResponse>("api/admin/settings/contact", request, cancellationToken);
}
