using BoschHomeVn.Api.Infrastructure;
using BoschHomeVn.Application.Settings;
using BoschHomeVn.Contracts.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Policy = AdminAuth.Policy)]
public sealed class AdminSettingsController(SiteSettingsHandler handler) : ControllerBase
{
    [HttpGet("contact")]
    public async Task<ContactSettingsResponse> GetContact(CancellationToken cancellationToken) =>
        ToResponse(await handler.GetContactAsync(cancellationToken));

    [HttpPut("contact")]
    public async Task<ContactSettingsResponse> SaveContact(SaveContactSettingsRequest request, CancellationToken cancellationToken) =>
        ToResponse(await handler.SaveContactAsync(new ContactSettings(request.Hotline, request.ZaloUrl), cancellationToken));

    private static ContactSettingsResponse ToResponse(ContactSettings s) => new(s.Hotline, s.ZaloUrl);
}
