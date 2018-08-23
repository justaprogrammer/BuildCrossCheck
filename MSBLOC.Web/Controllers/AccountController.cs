using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MSBLOC.Core.Interfaces;
using MSBLOC.Infrastructure.Interfaces;
using MSBLOC.Web.Interfaces;

namespace MSBLOC.Web.Controllers
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;

        public AccountController(IAccessTokenService accessTokenService)
        {
            _accessTokenService = accessTokenService;
        }
        
        [HttpGet("~/signin")]
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "GitHub");
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public async Task<IActionResult> SignOut()
        {
            var authProperties = new AuthenticationProperties { RedirectUri = "/" };
            await HttpContext.SignOutAsync(authProperties);
            return SignOut(authProperties);
        }

        public async Task<IActionResult> ListRepositories(
            [FromServices] IPersistantDataContext dbContext,
            [FromServices] IGitHubUserModelService gitHubUserModelService)
        {
            var installations = await gitHubUserModelService.GetInstallationsAsync();
            var repositories = installations.SelectMany(installation => installation.Repositories).ToArray();

            var issuedAccessTokens = await _accessTokenService.GetTokensForUserRepositoriesAsync();

            var tokenLookup = issuedAccessTokens.ToLookup(t => t.GitHubRepositoryId, r => r);

            ViewBag.TokenLookup = tokenLookup;
            ViewBag.Repositories = repositories;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateToken([FromQuery] long gitHubRepositoryId)
        {
            var jsonWebToken = await _accessTokenService.CreateTokenAsync(gitHubRepositoryId);

            return Content(jsonWebToken);
        }

        [HttpGet]
        public async Task<IActionResult> RevokeToken([FromQuery] Guid tokenId)
        {
            await _accessTokenService.RevokeTokenAsync(tokenId);

            return RedirectToAction("ListRepositories");
        }
    }
}
