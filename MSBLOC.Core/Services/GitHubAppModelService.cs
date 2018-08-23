using System;
using System.Linq;
using System.Threading.Tasks;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.LogAnalyzer;
using Newtonsoft.Json;
using Octokit;
using CheckRun = MSBLOC.Core.Model.GitHub.CheckRun;
using CheckWarningLevel = MSBLOC.Core.Model.LogAnalyzer.CheckWarningLevel;

namespace MSBLOC.Core.Services
{
    /// <inheritdoc cref="IGitHubAppModelService"/> />
    public class GitHubAppModelService : GitHubAppModelServiceBase, IGitHubAppModelService
    {
        private readonly IGitHubAppClientFactory _gitHubAppClientFactory;
        private readonly ITokenGenerator _tokenGenerator;

        public GitHubAppModelService(IGitHubAppClientFactory gitHubAppClientFactory, ITokenGenerator tokenGenerator)
        {
            _gitHubAppClientFactory = gitHubAppClientFactory;
            _tokenGenerator = tokenGenerator;
        }

        /// <inheritdoc />
        public async Task<CheckRun> CreateCheckRunAsync(string owner, string repository, string sha,
            string checkRunName,
            string checkRunTitle, string checkRunSummary, bool checkRunIsSuccess, Annotation[] annotations,
            DateTimeOffset? startedAt,
            DateTimeOffset? completedAt)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (sha == null) throw new ArgumentNullException(nameof(sha));
            if (checkRunTitle == null) throw new ArgumentNullException(nameof(checkRunTitle));
            if (checkRunSummary == null) throw new ArgumentNullException(nameof(checkRunSummary));

            if ((annotations?.Length ?? 0) > 50)
                throw new ArgumentException("Cannot create more than 50 annotations at a time");

            var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
            var checkRunsClient = gitHubClient?.Check?.Run;

            if (checkRunsClient == null) throw new InvalidOperationException("ICheckRunsClient is null");

            var newCheckRun = new NewCheckRun(checkRunName, sha)
            {
                Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                {
                    Annotations = annotations?
                        .Select(annotation => new NewCheckRunAnnotation(annotation.Filename, annotation.BlobHref,
                            annotation.LineNumber, annotation.EndLine, GetCheckWarningLevel(annotation),
                            annotation.Message))
                        .ToArray()
                },
                Status = CheckStatus.Completed,
                StartedAt = startedAt,
                CompletedAt = completedAt,
                Conclusion = checkRunIsSuccess ? CheckConclusion.Success : CheckConclusion.Failure
            };

            var checkRun = await checkRunsClient.Create(owner, repository, newCheckRun);

            return new CheckRun
            {
                Id = checkRun.Id,
                Url = checkRun.HtmlUrl,
            };
        }

        /// <inheritdoc />
        public async Task UpdateCheckRunAsync(long checkRunId, string owner, string repository,
            string sha, string checkRunTitle, string checkRunSummary, Annotation[] annotations,
            DateTimeOffset? startedAt, DateTimeOffset? completedAt)
        {
            if (annotations.Length > 50)
                throw new ArgumentException("Cannot create more than 50 annotations at a time");

            var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
            var checkRunsClient = gitHubClient?.Check?.Run;

            if (checkRunsClient == null) throw new InvalidOperationException("ICheckRunsClient is null");

            await checkRunsClient.Update(owner, repository, checkRunId, new CheckRunUpdate()
            {
                Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                {
                    Annotations = annotations
                        .Select(annotation => new NewCheckRunAnnotation(annotation.Filename, annotation.BlobHref,
                            annotation.LineNumber, annotation.EndLine, GetCheckWarningLevel(annotation),
                            annotation.Message))
                        .ToArray()
                }
            });
        }

        public async Task<string> GetRepositoryFileAsync(string owner, string repository, string path, string reference)
        {
            var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
            return await GetRepositoryFileAsync(gitHubClient, owner, repository, path, reference);
        }

        public async Task<LogAnalyzerConfiguration> GetLogAnalyzerConfigurationAsync(string owner, string repository, string reference)
        {
            var fileContent = await GetRepositoryFileAsync(owner, repository, "msbloc.json", reference);
            if (fileContent == null) return null;

            return JsonConvert.DeserializeObject<LogAnalyzerConfiguration>(fileContent);
        }

        private static Octokit.CheckWarningLevel GetCheckWarningLevel(Annotation annotation)
        {
            switch (annotation.CheckWarningLevel)
            {
                case CheckWarningLevel.Notice:
                    return Octokit.CheckWarningLevel.Notice;
                case CheckWarningLevel.Warning:
                    return Octokit.CheckWarningLevel.Warning;
                case CheckWarningLevel.Failure:
                    return Octokit.CheckWarningLevel.Failure;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}