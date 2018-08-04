using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Services;
using MSBLOC.Web.Models;
using Octokit;
using Octokit.GraphQL;
using Octokit.Internal;
using Activity = System.Diagnostics.Activity;

namespace MSBLOC.Web.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            if (User?.Identity.IsAuthenticated ?? false)
            {
                var gitHubClientFactory = new GitHubClientFactory(null);
                var gitHubName = User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
                var gitHubLogin = User.FindFirst(c => c.Type == "urn:github:login")?.Value;
                var gitHubUrl = User.FindFirst(c => c.Type == "urn:github:url")?.Value;
                var gitHubAvatar = User.FindFirst(c => c.Type == "urn:github:avatar")?.Value;

                string accessToken = await HttpContext.GetTokenAsync("access_token");

                var github = gitHubClientFactory.CreateClientForToken(accessToken);
                var repositories = await github.Repository.GetAllForCurrent();

                var connection = gitHubClientFactory.CreateGraphQlConnectionForToken(accessToken);
                var query = new Query().Viewer
                    .Repositories(null, null, null, null, null, null, null, null, null)
                    .AllPages()
                    .Select(repository => repository.Name);

                var repositoriesFromGraphQL = await connection.Run(query);
            }

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
        }
    }
}
