using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

// /api/* không có controller nào ở host này trả 404 thay vì trang Vue
public sealed class ApiNotFoundController : ControllerBase
{
    [Route("api/{**rest}")]
    public IActionResult Missing() => NotFound();
}
