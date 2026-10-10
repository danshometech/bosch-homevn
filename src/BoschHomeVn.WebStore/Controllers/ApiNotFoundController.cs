using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.WebStore.Controllers;

public sealed class ApiNotFoundController : ControllerBase
{
    [Route("api/{**rest}")]
    public IActionResult Missing() => NotFound();
}
