using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;
using Octokit;
using IGitHubClientFactory = MSBLOC.Web.Interfaces.IGitHubClientFactory;

namespace MSBLOC.Web.Controllers
{
    [Authorize]
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

        public async Task<IActionResult> ListRepositories([FromServices] IGitHubRepositoryContext repoContext, [FromServices] IGitHubClientFactory gitHubClientFactory)
        {
            var github = await gitHubClientFactory.CreateClientForCurrentUser();

            var repositories = (await github.Repository.GetAllForCurrent())
                .Select(r => new GitHubRepository
                {
                    Name = r.Name,
                    Id = r.Id,
                    Url = r.Url
                })
                .ToList();

            var filter = Builders<GitHubRepository>.Filter.In(nameof(GitHubRepository.Id), repositories.Select(r => r.Id));

            var savedRepos = await repoContext.Repositories.Find(filter).ToListAsync();

            foreach (var savedRepo in savedRepos)
            {
                var repo = repositories.FirstOrDefault(r => r.Id == savedRepo.Id);
                if (repo == null) continue;
                repo.Id = savedRepo.Id;
                repo.Secret = savedRepo.Secret;
            }

            return View(repositories);
        }

        [HttpGet]
        public async Task<IActionResult> CreateSecret([FromServices] IGitHubRepositoryContext repoContext, [FromServices] IGitHubClientFactory gitHubClientFactory, [FromQuery] long gitHubRepositoryId)
        {
            var github = await gitHubClientFactory.CreateClientForCurrentUser();

            var repoTask = github.Repository.Get(gitHubRepositoryId);

            var filter = Builders<GitHubRepository>.Filter.Eq(nameof(GitHubRepository.Id), gitHubRepositoryId);
            var repo = await repoContext.Repositories.Find(filter).FirstOrDefaultAsync();

            var repository = await repoTask;

            if (repo == null)
            {
                repo = new GitHubRepository
                {
                    Id = repository.Id,
                    Name = repository.Name,
                    Url = repository.Url,
                    Secret = Guid.NewGuid().ToString()
                };

                await repoContext.Repositories.InsertOneAsync(repo);
            }
            else
            {
                repo.Name = repository.Name;
                repo.Url = repository.Url;
                repo.Secret = Guid.NewGuid().ToString();

                await repoContext.Repositories.ReplaceOneAsync(filter, repo);
            }

            return RedirectToAction("ListRepositories");
        }
    }
}
