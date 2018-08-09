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
            [FromServices] ITokenGenerator tokenGenerator, 
            [FromServices] IGitHubAppClientFactory gitHubAppClientFactory, 
            [FromServices] IGitHubUserClientFactory gitHubUserClientFactory)
        {
            var gitHubAppClient = gitHubAppClientFactory.CreateAppClient(tokenGenerator);

            var userClient = await gitHubUserClientFactory.CreateClient();
            var gitHubAppsUserClient = userClient.GitHubApps;

            InstallationsResponse installations = await gitHubAppsUserClient.GetAllInstallationsForUser();
            var repositories = new List<Repository>();
            foreach (var installation in installations.Installations)
            {
                switch (installation.TargetType.Value)
                {
                    case AccountType.Organization:
                        repositories.AddRange(await userClient.Repository.GetAllForOrg(installation.Account.Login));
                        break;

                    case AccountType.User:
                    case AccountType.Bot:
                        throw new NotSupportedException();

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var tokenLookup = new Dictionary<long, AccessToken>()
                .ToLookup(pair => pair.Key, pair => pair.Value);

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
