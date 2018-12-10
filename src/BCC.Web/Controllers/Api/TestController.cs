using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BCC.Web.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    public class TestController : ApiControllerBase
    {
        [HttpGet]
        [Route("authentication")]
        [ExcludeFromCodeCoverage]
        public IActionResult Authentication()
        {
            return Json(new
            {
                User.Identity.IsAuthenticated,
                Claims = User.Claims.ToDictionary(c => c.Type, c => c.Value)
            });
        }
    }
}