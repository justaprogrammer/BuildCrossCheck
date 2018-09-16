using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MoreLinq.Extensions;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Model.CheckRunSubmission;
using MSBLOC.Core.Model.LogAnalyzer;
using Newtonsoft.Json;
using Octokit;

namespace MSBLOC.Core.Services.GitHub
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

        public async Task<Model.GitHub.CheckRun> SubmitCheckRunAsync(string owner,
            string repository, string headSha, string name,
            string title, string summary, bool success,
            Annotation[] annotations, DateTimeOffset startedAt, DateTimeOffset completedAt)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("Owner is invalid", nameof(owner));
            }

            if (string.IsNullOrWhiteSpace(repository))
            {
                throw new ArgumentException("Name is invalid", nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(headSha))
            {
                throw new ArgumentException("HeadSha is invalid", nameof(headSha));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Name is invalid", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title is invalid", nameof(title));
            }

            var annotationBatches = annotations?.Batch(50).ToArray();

            var checkRun = await CreateCheckRunAsync(owner, repository, headSha, name,
                    title, summary, success, annotationBatches?.FirstOrDefault()?.ToArray(), startedAt, completedAt)
                .ConfigureAwait(false);

            if (annotationBatches != null)
            {
                foreach (var annotationBatch in annotationBatches.Skip(1))
                {
                    await UpdateCheckRunAsync(checkRun.Id, owner, repository, headSha, title,
                        summary, annotationBatch.ToArray(), startedAt, completedAt).ConfigureAwait(false);
                }
            }

            return checkRun;
        }

        /// <inheritdoc />
        public async Task<Model.GitHub.CheckRun> CreateCheckRunAsync(string owner, string repository, string sha,
            string checkRunName, string checkRunTitle, string checkRunSummary,
            bool checkRunIsSuccess, Annotation[] annotations,
            DateTimeOffset? startedAt, DateTimeOffset? completedAt)
        {
            try
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
                            .Select(annotation => new NewCheckRunAnnotation(annotation.Filename,
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

                return new MSBLOC.Core.Model.GitHub.CheckRun
                {
                    Id = checkRun.Id,
                    Url = checkRun.HtmlUrl,
                };
            }
            catch (Exception ex)
            {
                throw new GitHubAppModelException("Error creating CheckRun.", ex);
            }
        }

        /// <inheritdoc />
        public async Task UpdateCheckRunAsync(long checkRunId, string owner, string repository,
            string sha, string checkRunTitle, string checkRunSummary, Annotation[] annotations,
            DateTimeOffset? startedAt, DateTimeOffset? completedAt)
        {
            try
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
                            .Select(annotation => new NewCheckRunAnnotation(annotation.Filename,
                                annotation.LineNumber, annotation.EndLine, GetCheckWarningLevel(annotation),
                                annotation.Message))
                            .ToArray()
                    }
                });
            }
            catch (Exception ex)
            {
                throw new GitHubAppModelException("Error updating CheckRun.", ex);
            }
        }

        public async Task<string> GetRepositoryFileAsync(string owner, string repository, string path, string reference)
        {
            try
            {
                var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
                return await GetRepositoryFileAsync(gitHubClient, owner, repository, path, reference);
            }
            catch (Exception ex)
            {
                throw new GitHubAppModelException("Error getting repository file.", ex);
            }
        }

        private static CheckAnnotationLevel GetCheckWarningLevel(Annotation annotation)
        {
            switch (annotation.CheckWarningLevel)
            {
                case Model.LogAnalyzer.CheckWarningLevel.Notice:
                    return CheckAnnotationLevel.Notice;
                case Model.LogAnalyzer.CheckWarningLevel.Warning:
                    return CheckAnnotationLevel.Warning;
                case Model.LogAnalyzer.CheckWarningLevel.Failure:
                    return CheckAnnotationLevel.Failure;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}