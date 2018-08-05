using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;

namespace MSBLOC.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("~/signin")]
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "GitHub");
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        [Authorize]
        public async Task<IActionResult> SignOut()
        {
            var authProperties = new AuthenticationProperties {RedirectUri = "/"};
            await HttpContext.SignOutAsync(authProperties);
            return SignOut(authProperties);
        }

        [Authorize]
        public async Task<IActionResult> ListRepositories()
        {
            var gitHubClientFactory = new GitHubClientFactory();
            var gitHubName = User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
            var gitHubLogin = User.FindFirst(c => c.Type == "urn:github:login")?.Value;
            var gitHubUrl = User.FindFirst(c => c.Type == "urn:github:url")?.Value;
            var gitHubAvatar = User.FindFirst(c => c.Type == "urn:github:avatar")?.Value;

            var accessToken = await HttpContext.GetTokenAsync("access_token");

            var github = gitHubClientFactory.CreateClient(accessToken);
            var repositories = await github.Repository.GetAllForCurrent();

            return View(repositories);
        }
    }
}
