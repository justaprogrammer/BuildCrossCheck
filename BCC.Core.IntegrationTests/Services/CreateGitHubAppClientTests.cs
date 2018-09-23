namespace BCC.Core.IntegrationTests.Services
{
    public class CreateGitHubAppClientTests : IntegrationTestsBase
    {
        [IntegrationTest]
        public async Task ShouldQueryRepositoryInstallations()
        {
            var gitHubClient = CreateGitHubAppClient();
            var gitHubAppsClient = gitHubClient.GitHubApps;

            var repositoryInstallation = await gitHubAppsClient.GetRepositoryInstallationForCurrent(TestAppOwner, TestAppRepo);
            repositoryInstallation.Id.Should().Be(TestAppInstallationId);
        }

        [IntegrationTest]
        public async Task ShouldFindInstallationById()
        {
            var gitHubClient = CreateGitHubAppClient();
            var gitHubAppsClient = gitHubClient.GitHubApps;

            var installation = await gitHubAppsClient.GetInstallationForCurrent(TestAppInstallationId);
            installation.Id.Should().Be(TestAppInstallationId);
        }

        [IntegrationTest]
        public async Task ShouldFindAllInstallation()
        {
            var gitHubClient = CreateGitHubAppClient();
            var gitHubAppsClient = gitHubClient.GitHubApps;

            var installations = await gitHubAppsClient.GetAllInstallationsForCurrent();
            installations.Any(installation => installation.Id == TestAppInstallationId).Should().BeTrue();
        }
    }
}