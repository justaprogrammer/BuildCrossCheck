using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using Nito.AsyncEx;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class GitHubUserModelService : IGitHubUserModelService
    {
        private readonly AsyncLazy<IGitHubClient> _lazyGitHubUserClient;

        public GitHubUserModelService(IGitHubUserClientFactory gitHubUserClientFactory)
        {
            _lazyGitHubUserClient = new AsyncLazy<IGitHubClient>(() => gitHubUserClientFactory.CreateClient());
        }

        public async Task<IReadOnlyList<UserInstallation>> GetUserInstallations()
        {
            var gitHubUserClient = await _lazyGitHubUserClient;
            var gitHubAppsUserClient = gitHubUserClient.GitHubApps;
            var gitHubAppsInstallationsUserClient = gitHubAppsUserClient.Installations;

            var userInstallations = new List<UserInstallation>();

            var installationsResponse = await gitHubAppsUserClient.GetAllInstallationsForUser().ConfigureAwait(false);

            foreach (var installation in installationsResponse.Installations)
            {
                var repositoriesResponse = await gitHubAppsInstallationsUserClient
                    .GetAllRepositoriesForUser(installation.Id).ConfigureAwait(false);

                var userInstallation = new UserInstallation
                {
                    Id = installation.Id,
                    Login = installation.Account.Login,
                    Repositories = repositoriesResponse.Repositories
                        .Select(repository => new UserRepository
                        {
                            Id = repository.Id,
                            Owner = repository.Owner.Login,
                            Name = repository.Name,
                            Url = repository.Url
                        })
                        .ToArray()
                };

                userInstallations.Add(userInstallation);
            }

            return userInstallations;
        }

        public async Task<UserInstallation> GetUserInstallation(long installationId)
        {
            var gitHubUserClient = await _lazyGitHubUserClient.ConfigureAwait(false);
            var gitHubAppsUserClient = gitHubUserClient.GitHubApps;
            var gitHubAppsInstallationsUserClient = gitHubAppsUserClient.Installations;

            var installation = await gitHubAppsUserClient.GetInstallation(installationId).ConfigureAwait(false);
            var repositoriesResponse = await gitHubAppsInstallationsUserClient
                .GetAllRepositoriesForUser(installation.Id).ConfigureAwait(false);

            var repositoriesResponseRepositories = repositoriesResponse.Repositories;

            var userInstallation = BuildUserInstallation(installation, repositoriesResponseRepositories);

            return userInstallation;
        }

        public async Task<IReadOnlyList<UserRepository>> GetUserRepositories()
        {
            var userInstallations = await GetUserInstallations().ConfigureAwait(false);
            return userInstallations.SelectMany(installation => installation.Repositories).ToArray();
        }

        public async Task<UserRepository> GetUserRepository(long repositoryId)
        {
            var gitHubUserClient = await _lazyGitHubUserClient.ConfigureAwait(false);
            var repositoriesClient = gitHubUserClient.Repository;

            var repository = await repositoriesClient.Get(repositoryId).ConfigureAwait(false);
            return BuildUserRepository(repository);
        }

        private static UserInstallation BuildUserInstallation(Installation installation,
            IReadOnlyList<Repository> repositories)
        {
            return new UserInstallation
            {
                Id = installation.Id,
                Login = installation.Account.Login,
                Repositories = repositories
                    .Select(BuildUserRepository)
                    .ToArray()
            };
        }

        private static UserRepository BuildUserRepository(Repository repository)
        {
            return new UserRepository
            {
                Id = repository.Id,
                Owner = repository.Owner.Login,
                Name = repository.Name,
                Url = repository.Url
            };
        }
    }
}