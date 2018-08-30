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

            var repositoryInstallation = await gitHubAppsClient.GetRepositoryInstallationForCurrent(TestAppOwner, TestAppRepo);
            repositoryInstallation.Id.ToString().Should().Be(TestAppInstallationId);
        }

        [IntegrationTest]
        public async Task ShouldFindRepositoryInstallationById()
        {
            var gitHubClient = CreateGitHubAppClient();
            var gitHubAppsClient = gitHubClient.GitHubApps;

            var repositoryInstallation = await gitHubAppsClient.GetRepositoryInstallationForCurrent(long.Parse(TestAppInstallationId));
            repositoryInstallation.Id.ToString().Should().Be(TestAppInstallationId);
        }
    }
}