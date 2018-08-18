using Microsoft.AspNetCore.Mvc;
using MSBLOC.Web.Models;
using Activity = System.Diagnostics.Activity;

namespace MSBLOC.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
