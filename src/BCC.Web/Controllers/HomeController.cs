using System.Diagnostics;
using BCC.Web.Services;
using BCC.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BCC.Web.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly ITelemetryService _telemetryService;

        public HomeController(ITelemetryService telemetryService)
        {
            _telemetryService = telemetryService;
        }

        public IActionResult Index()
        {
            _telemetryService.TrackPageView(Pages.Home);

            var viewModelBase = new ViewModelBase();
            return View(viewModelBase);
        }

        public IActionResult Error()
        {
            _telemetryService.TrackPageView(Pages.Error);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
