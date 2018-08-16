using System;
using System.Threading.Tasks;
using FluentAssertions;
using MSBLOC.Core.IntegrationTests.Utilities;

namespace MSBLOC.Core.IntegrationTests.Services
{
    public class CreateGitHubAppClientTests : IntegrationTestsBase
    {
        [IntegrationTest]
        public async Task ShouldQueryRepositoryInstallations()
        {
            var gitHubClient = CreateGitHubAppClient();
            var gitHubAppsClient = gitHubClient.GitHubApps;

            var repositoryInstallation = await gitHubAppsClient.GetRepositoryInstallation(TestAppOwner, TestAppRepo);
            repositoryInstallation.Id.ToString().Should().Be(TestAppInstallationId);
        }

        [IntegrationTest]
        public async Task ShouldFindRepositoryInstallationById()
        {
            var gitHubClient = CreateGitHubAppClient();
            var gitHubAppsClient = gitHubClient.GitHubApps;

            var repositoryInstallation = await gitHubAppsClient.GetInstallation(Int64.Parse(TestAppInstallationId));
            repositoryInstallation.Id.ToString().Should().Be(TestAppInstallationId);
        }
    }
}