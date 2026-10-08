using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BoschHomeVn.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController(HealthCheckService healthChecks) : ControllerBase
{
    // Liveness cho load balancer/monitor. Check DB thêm sau bằng AddHealthChecks().AddDbContextCheck<AppDbContext>()
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        var body = new { status = report.Status.ToString() };

        return report.Status == HealthStatus.Unhealthy
            ? StatusCode(StatusCodes.Status503ServiceUnavailable, body)
            : Ok(body);
    }
}
