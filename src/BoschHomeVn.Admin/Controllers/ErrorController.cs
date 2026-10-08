using System.Diagnostics;
using BoschHomeVn.Admin.Models;
using Microsoft.AspNetCore.Mvc;

namespace BoschHomeVn.Admin.Controllers;

// Trang lỗi phía server (ngoài môi trường Development). Lỗi bên trong app Vue do Vue tự hiển thị.
public sealed class ErrorController : Controller
{
    [Route("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Index() =>
        View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
