using Microsoft.AspNetCore.Mvc;
using MSBLOC.Web.Models;
using MSBLOC.Web.ViewModels;
using Activity = System.Diagnostics.Activity;

namespace MSBLOC.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var viewModelBase = new ViewModelBase();
            return View(viewModelBase);
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
