using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MSBLOC.Core.Interfaces;
using MSBLOC.Web.Interfaces;
using Octokit;
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
            var authProperties = new AuthenticationProperties {RedirectUri = "/"};
            await HttpContext.SignOutAsync(authProperties);
            return SignOut(authProperties);
        }

        public async Task<IActionResult> ListRepositories(
            [FromServices] IPersistantDataContext dbContext, 
            [FromServices] IGitHubUserClientFactory gitHubUserClientFactory)
        {
            var userClient = await gitHubUserClientFactory.CreateClient();
            var gitHubAppsUserClient = userClient.GitHubApps;
            var gitHubAppsInstallationsUserClient = gitHubAppsUserClient.Installations;

            var repositories = new List<Repository>();

            var installationsResponse = await gitHubAppsUserClient.GetAllInstallationsForUser();
            foreach (var installation in installationsResponse.Installations)
            {
                var repositoriesResponse = await gitHubAppsInstallationsUserClient.GetAllRepositoriesForUser(installation.Id);
                repositories.AddRange(repositoriesResponse.Repositories);
            }

            var filter = Builders<AccessToken>.Filter.In(nameof(AccessToken.GitHubRepositoryId), repositories.Select(r => r.Id));

            var issuedAccessTokens = await dbContext.AccessTokens.Find(filter).ToListAsync();

            var tokenLookup = issuedAccessTokens.ToLookup(t => t.GitHubRepositoryId, r => r);

            ViewBag.TokenLookup = tokenLookup;
            ViewBag.Repositories = repositories;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CreateToken([FromServices] IPersistantDataContext dbContext, [FromServices] IGitHubUserClientFactory gitHubClientFactory, [FromServices] IJsonWebTokenService tokenService, [FromQuery] long gitHubRepositoryId)
        {
            var github = await gitHubClientFactory.CreateClient();

            var repository = await github.Repository.Get(gitHubRepositoryId);

            if (repository == null)
            {
                return NotFound();
            }

            var (accessToken, jsonWebToken) = tokenService.CreateToken(User, repository.Id);

            await dbContext.AccessTokens.InsertOneAsync(accessToken);

            return Content(jsonWebToken);
        }

        [HttpGet]
        public async Task<IActionResult> RevokeToken([FromServices] IPersistantDataContext dbContext, [FromServices] IGitHubUserClientFactory gitHubClientFactory, [FromQuery] Guid tokenId)
        {
            var github = await gitHubClientFactory.CreateClient();

            var repositories = (await github.Repository.GetAllForCurrent()).ToList();

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
