using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MSBLOC.Core.Interfaces;
using MSBLOC.Web.Interfaces;
using AccessToken = MSBLOC.Web.Models.AccessToken;

namespace MSBLOC.Web.Controllers
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
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
            var userInstallations = await gitHubUserModelService.GetUserInstallations();
            var repositories = userInstallations.SelectMany(installation => installation.Repositories).ToArray();

            var filter = Builders<AccessToken>.Filter.In(nameof(AccessToken.GitHubRepositoryId), repositories.Select(r => r.Id));

            var issuedAccessTokens = await dbContext.AccessTokens.Find(filter).ToListAsync();

            var tokenLookup = issuedAccessTokens.ToLookup(t => t.GitHubRepositoryId, r => r);

            ViewBag.TokenLookup = tokenLookup;
            ViewBag.Repositories = repositories;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateToken(
            [FromServices] IPersistantDataContext dbContext,
            [FromServices] IGitHubUserModelService gitHubUserModelService,
            [FromServices] IJsonWebTokenService tokenService,
            [FromQuery] long installationId,
            [FromQuery] long repositoryId)
        {

            var userInstallation = await gitHubUserModelService.GetUserInstallation(installationId);
            var repository = userInstallation.Repositories.FirstOrDefault(userRepository => userRepository.Id == repositoryId);
            if (repository == null)
            {
                return NotFound();
            }

            var (accessToken, jsonWebToken) = tokenService.CreateToken(User, repository.Id);

            await dbContext.AccessTokens.InsertOneAsync(accessToken);

            return Content(jsonWebToken);
        }

        [HttpGet]
        public async Task<IActionResult> RevokeToken(
            [FromServices] IPersistantDataContext dbContext,
            [FromServices] IGitHubUserModelService gitHubUserModelService, 
            [FromQuery] Guid tokenId)
        {
            var userInstallations = await gitHubUserModelService.GetUserInstallations();
            var repositories = userInstallations.SelectMany(installation => installation.Repositories).ToArray();

            var filter = Builders<AccessToken>.Filter.Eq(nameof(AccessToken.Id), tokenId);

            var token = await dbContext.AccessTokens.Find(filter).FirstAsync();

            if (repositories.Select(r => r.Id).Contains(token.GitHubRepositoryId))
            {
                await dbContext.AccessTokens.DeleteOneAsync(filter);
            }

            return RedirectToAction("ListRepositories");
        }
    }
}
