using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces.GitHub;
using Nito.AsyncEx;
using Octokit;
using AccountType = MSBLOC.Core.Model.GitHub.AccountType;
using Installation = MSBLOC.Core.Model.GitHub.Installation;
using Repository = MSBLOC.Core.Model.GitHub.Repository;

namespace MSBLOC.Core.Services.GitHub
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
            var gitHubAppsInstallationsUserClient = gitHubAppsUserClient.Installation;

            var userInstallations = new List<Installation>();

            var installationsResponse = await gitHubAppsUserClient.GetAllInstallationsForCurrentUser().ConfigureAwait(false);

            foreach (var installation in installationsResponse.Installations)
            {
                var repositoriesResponse = await gitHubAppsInstallationsUserClient
                    .GetAllRepositoriesForCurrentUser(installation.Id).ConfigureAwait(false);

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
                            OwnerUrl = repository.Owner.HtmlUrl,
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