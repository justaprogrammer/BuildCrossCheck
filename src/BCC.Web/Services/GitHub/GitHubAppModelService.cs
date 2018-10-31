using System;
using System.Linq;
using System.Threading.Tasks;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Web.Interfaces.GitHub;
using MoreLinq.Extensions;
using Octokit;
using CheckWarningLevel = BCC.Core.Model.CheckRunSubmission.CheckWarningLevel;

namespace BCC.Web.Services.GitHub
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
        public async Task<Web.Models.GitHub.CheckRun> SubmitCheckRunAsync(string owner,
            string repository, string sha, string name,
            string title, string summary, bool success,
            Annotation[] annotations, DateTimeOffset startedAt, DateTimeOffset completedAt)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("Owner is invalid", nameof(owner));
            }

            if (string.IsNullOrWhiteSpace(repository))
            {
                throw new ArgumentException("Repository is invalid", nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(sha))
            {
                throw new ArgumentException("HeadSha is invalid", nameof(sha));
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

            var checkRun = await CreateCheckRunAsync(owner, repository, sha, name,
                    title, summary, success, annotationBatches?.FirstOrDefault()?.ToArray(), startedAt, completedAt)
                .ConfigureAwait(false);

            if (annotationBatches != null)
            {
                foreach (var annotationBatch in annotationBatches.Skip(1))
                {
                    await UpdateCheckRunAsync(checkRun.Id, owner, repository, title,
                        summary, annotationBatch.ToArray()).ConfigureAwait(false);
                }
            }

            return checkRun;
        }

        /// <summary>
        /// Creates a CheckRun in the GitHub Api.
        /// </summary>
        /// <param name="owner">The name of the repository owner.</param>
        /// <param name="repository">The name of the repository.</param>
        /// <param name="sha">The sha we are creating this CheckRun for.</param>
        /// <param name="name">The name of the CheckRun.</param>
        /// <param name="title">The title of the CheckRun.</param>
        /// <param name="summary">The summary of the CheckRun.</param>
        /// <param name="success">If the CheckRun is a success.</param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <param name="startedAt">The time when processing started</param>
        /// <param name="completedAt">The time when processing finished</param>
        /// <returns></returns>
        public async Task<Web.Models.GitHub.CheckRun> CreateCheckRunAsync(string owner, string repository, string sha,
            string name, string title, string summary,
            bool success, Annotation[] annotations,
            DateTimeOffset? startedAt, DateTimeOffset? completedAt)
        {
            try
            {
                if (owner == null) throw new ArgumentNullException(nameof(owner));
                if (repository == null) throw new ArgumentNullException(nameof(repository));
                if (sha == null) throw new ArgumentNullException(nameof(sha));
                if (title == null) throw new ArgumentNullException(nameof(title));
                if (summary == null) throw new ArgumentNullException(nameof(summary));

                if ((annotations?.Length ?? 0) > 50)
                    throw new ArgumentException("Cannot create more than 50 annotations at a time");

                var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
                var checkRunsClient = gitHubClient?.Check?.Run;

                if (checkRunsClient == null) throw new InvalidOperationException("ICheckRunsClient is null");

                var newCheckRun = new NewCheckRun(name, sha)
                {
                    Output = new NewCheckRunOutput(title, summary)
                    {
                        Text = "##Custom Text\r\n- Also Markdown\r\n- What do I use this for?",
                        Images = new[]
                        {
                            new NewCheckRunImage("What is this for too", "http://www.adpestelimination.com/images/pests/squirreltail.gif")
                        },
                        Annotations = annotations?
                            .Select(CreateNewCheckRunAnnotation)
                            .ToArray()
                    },
                    Status = CheckStatus.Completed,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    Conclusion = success ? CheckConclusion.Success : CheckConclusion.Failure
                };

                var checkRun = await checkRunsClient.Create(owner, repository, newCheckRun);

                return new Web.Models.GitHub.CheckRun
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

        /// <summary>
        /// Updates a CheckRun in the GitHub Api.
        /// </summary>
        /// <param name="checkRunId">The id of the CheckRun being updated.</param>
        /// <param name="owner">The name of the repository owner.</param>
        /// <param name="repository">The name of the repository</param>
        /// <param name="title">The title of the CheckRun.</param>
        /// <param name="summary">The summary of the CheckRun.</param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <returns></returns>
        public async Task UpdateCheckRunAsync(long checkRunId, string owner, string repository, 
            string title, string summary, Annotation[] annotations)
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
                    Output = new NewCheckRunOutput(title, summary)
                    {
                        Annotations = annotations
                            .Select(CreateNewCheckRunAnnotation)
                            .ToArray()
                    }
                });
            }
            catch (Exception ex)
            {
                throw new GitHubAppModelException("Error updating CheckRun.", ex);
            }
        }

        private static NewCheckRunAnnotation CreateNewCheckRunAnnotation(Annotation annotation)
        {
            var newCheckRunAnnotation = new NewCheckRunAnnotation(annotation.Filename,
                annotation.LineNumber, annotation.EndLine, GetCheckWarningLevel(annotation),
                annotation.Message);

            if (!string.IsNullOrWhiteSpace(annotation.Title))
            {
                newCheckRunAnnotation.Title = annotation.Title;
            }

            newCheckRunAnnotation.RawDetails = "##Even More PlainText\r\n- No Markdown Here.\r\n- What? Why?";

            return newCheckRunAnnotation;
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
                case CheckWarningLevel.Notice:
                    return CheckAnnotationLevel.Notice;
                case CheckWarningLevel.Warning:
                    return CheckAnnotationLevel.Warning;
                case CheckWarningLevel.Failure:
                    return CheckAnnotationLevel.Failure;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}