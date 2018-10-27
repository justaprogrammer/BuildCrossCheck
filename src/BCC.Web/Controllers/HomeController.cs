using System.Diagnostics;
using BCC.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BCC.Web.Controllers
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
