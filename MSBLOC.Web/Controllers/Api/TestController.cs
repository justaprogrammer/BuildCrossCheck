using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MSBLOC.Web.Controllers.Api
{
    [Authorize]
    [Route("api/[controller]")]
    public class TestController : ApiControllerBase
    {
        [HttpGet]
        [Route("test")]
        [ExcludeFromCodeCoverage]
        public IActionResult TestAuthentication()
        {
            return Json(new
            {
                User.Identity.IsAuthenticated,
                Claims = User.Claims.ToDictionary(c => c.Type, c => c.Value)
            });
        }
    }
}