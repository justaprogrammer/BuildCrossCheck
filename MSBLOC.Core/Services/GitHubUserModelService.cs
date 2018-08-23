using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.GitHub;
using Nito.AsyncEx;
using Octokit;
using AccountType = MSBLOC.Core.Model.GitHub.AccountType;
using Installation = MSBLOC.Core.Model.GitHub.Installation;
using Repository = MSBLOC.Core.Model.GitHub.Repository;

namespace MSBLOC.Core.Services
{
    /// <inheritdoc />
    public class GitHubUserModelService : IGitHubUserModelService
    {
        private readonly AsyncLazy<IGitHubClient> _lazyGitHubUserClient;
        private readonly AsyncLazy<IGitHubGraphQLClient> _lazyGitHubUserGraphQLClient;

        public GitHubUserModelService(IGitHubUserClientFactory gitHubUserClientFactory)
        {
            _lazyGitHubUserClient = new AsyncLazy<IGitHubClient>(() => gitHubUserClientFactory.CreateClient());
            _lazyGitHubUserGraphQLClient = new AsyncLazy<IGitHubGraphQLClient>(() => gitHubUserClientFactory.CreateGraphQLClient());
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Installation>> GetInstallationsAsync()
        {
            var gitHubUserClient = await _lazyGitHubUserClient;
            var gitHubAppsUserClient = gitHubUserClient.GitHubApps;
            var gitHubAppsInstallationsUserClient = gitHubAppsUserClient.Installations;

            var userInstallations = new List<Installation>();

            var installationsResponse = await gitHubAppsUserClient.GetAllInstallationsForUser().ConfigureAwait(false);

            foreach (var installation in installationsResponse.Installations)
            {
                var repositoriesResponse = await gitHubAppsInstallationsUserClient
                    .GetAllRepositoriesForUser(installation.Id).ConfigureAwait(false);

                var userInstallation = new Installation
                {
                    Id = installation.Id,
                    Login = installation.Account.Login,
                    Repositories = repositoriesResponse.Repositories
                        .Select(repository => new Repository
                        {
                            Id = repository.Id,
                            NodeId = repository.NodeId,
                            OwnerId = repository.Owner.Id,
                            OwnerNodeId = repository.Owner.NodeId,
                            OwnerType = GetAccountType(repository),
                            Owner = repository.Owner.Login,
                            Name = repository.Name,
                            Url = repository.HtmlUrl
                        })
                        .ToArray()
                };

                userInstallations.Add(userInstallation);
            }

            return userInstallations;
        }

        /// <inheritdoc />
        public async Task<Installation> GetInstallationAsync(long installationId)
        {
            var gitHubUserClient = await _lazyGitHubUserClient.ConfigureAwait(false);
            var gitHubAppsUserClient = gitHubUserClient.GitHubApps;
            var gitHubAppsInstallationsUserClient = gitHubAppsUserClient.Installations;

            var installation = await gitHubAppsUserClient.GetInstallation(installationId).ConfigureAwait(false);
            var repositoriesResponse = await gitHubAppsInstallationsUserClient
                .GetAllRepositoriesForUser(installation.Id).ConfigureAwait(false);

            var repositoriesResponseRepositories = repositoriesResponse.Repositories;

            var userInstallation = BuildInstallation(installation, repositoriesResponseRepositories);

            return userInstallation;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Repository>> GetRepositoriesAsync()
        {
            var userInstallations = await GetInstallationsAsync().ConfigureAwait(false);
            return userInstallations.SelectMany(installation => installation.Repositories).ToArray();
        }

        /// <inheritdoc />
        public async Task<Repository> GetRepositoryAsync(long repositoryId)
        {
            var gitHubUserClient = await _lazyGitHubUserClient.ConfigureAwait(false);
            var repositoriesClient = gitHubUserClient.Repository;

            var repository = await repositoriesClient.Get(repositoryId).ConfigureAwait(false);
            return BuildRepository(repository);
        }

        private static Installation BuildInstallation(Octokit.Installation installation,
            IReadOnlyList<Octokit.Repository> repositories)
        {
            return new Installation
            {
                Id = installation.Id,
                Login = installation.Account.Login,
                Repositories = repositories
                    .Select(BuildRepository)
                    .ToArray()
            };
        }

        private static Repository BuildRepository(Octokit.Repository repository)
        {
            return new Repository
            {
                Id = repository.Id,
                Owner = repository.Owner.Login,
                Name = repository.Name,
                Url = repository.Url
            };
        }

        private static AccountType GetAccountType(Octokit.Repository repository)
        {
            switch (repository.Owner.Type)
            {
                case Octokit.AccountType.User:
                    return AccountType.User;

                case Octokit.AccountType.Organization:
                    return AccountType.Organization;

                case Octokit.AccountType.Bot:
                    throw new InvalidOperationException("A bot cannot own a repository.");

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}