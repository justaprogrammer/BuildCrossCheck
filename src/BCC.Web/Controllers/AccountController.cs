﻿using System;
using System.Linq;
using System.Threading.Tasks;
using BCC.Web.Authentication;
using BCC.Web.Interfaces;
using BCC.Web.Services;
using BCC.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IGitHubUserModelService = BCC.Web.Interfaces.GitHub.IGitHubUserModelService;

namespace BCC.Web.Controllers
{
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AccountController : Controller
    {
        private readonly IAccessTokenService _accessTokenService;
        private readonly ITelemetryService _telemetryService;

        public AccountController(IAccessTokenService accessTokenService, ITelemetryService telemetryService)
        {
            _accessTokenService = accessTokenService;
            _telemetryService = telemetryService;
        }
        
        [HttpGet("~/signin")]
        [AllowAnonymous]
        public IActionResult SignIn()
        {
            _telemetryService.TrackPageView(Pages.SignIn);

            return Challenge(new AuthenticationProperties { RedirectUri = "/" }, "GitHub");
        }

        [HttpGet("~/signout"), HttpPost("~/signout")]
        public async Task<IActionResult> SignOut()
        {
            _telemetryService.TrackPageView(Pages.SignOut);

            var authProperties = new AuthenticationProperties { RedirectUri = "/" };
            await HttpContext.SignOutAsync(authProperties);
            return SignOut(authProperties);
        }

        public async Task<IActionResult> ListRepositories([FromServices] IGitHubUserModelService gitHubUserModelService)
        {
            _telemetryService.TrackPageView(Pages.ListRepositories);

            var listRepositoriesViewModel = await BuildListRepositoriesViewModel(gitHubUserModelService);

            return View(listRepositoriesViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> CreateToken([FromServices] IGitHubUserModelService gitHubUserModelService, [FromQuery] long gitHubRepositoryId)
        {
            _telemetryService.CreateToken(User.FindFirst(CustomClaims.GithubLogin).Value);

            var jsonWebToken = await _accessTokenService.CreateTokenAsync(gitHubRepositoryId);
            var listRepositoriesViewModel = await BuildListRepositoriesViewModel(gitHubUserModelService, gitHubRepositoryId, jsonWebToken);

            return View("ListRepositories", listRepositoriesViewModel);
        }

        private async Task<ListRepositoriesViewModel> BuildListRepositoriesViewModel(IGitHubUserModelService gitHubUserModelService)
        {
            var installations = await gitHubUserModelService.GetInstallationsAsync();

            var repositoriesByOwner = installations
                .SelectMany(installation => installation.Repositories)
                .GroupBy(repository => repository.Owner)
                .OrderBy(grouping => grouping.Key)
                .ToArray();

            var issuedAccessTokens = await _accessTokenService.GetTokensForUserRepositoriesAsync();

            var tokenLookup = issuedAccessTokens.ToLookup(t => t.GitHubRepositoryId, r => r);

            return new ListRepositoriesViewModel
            {
                TokenLookup = tokenLookup,
                RepositoriesByOwner = repositoriesByOwner
            };
        }

        private async Task<ListRepositoriesViewModel> BuildListRepositoriesViewModel(IGitHubUserModelService gitHubUserModelService, long gitHubRepositoryId, string jsonWebToken)
        {
            var buildListRepositoriesViewModel = await BuildListRepositoriesViewModel(gitHubUserModelService);
            buildListRepositoriesViewModel.CreatedToken = jsonWebToken;
            buildListRepositoriesViewModel.CreatedTokenRepoId = gitHubRepositoryId;
            return buildListRepositoriesViewModel;
        }

        [HttpGet]
        public async Task<IActionResult> RevokeToken([FromQuery] Guid tokenId)
        {
            _telemetryService.RevokeToken(User.FindFirst(CustomClaims.GithubLogin).Value);

            await _accessTokenService.RevokeTokenAsync(tokenId);

            return RedirectToAction("ListRepositories");
        }
    }
}
