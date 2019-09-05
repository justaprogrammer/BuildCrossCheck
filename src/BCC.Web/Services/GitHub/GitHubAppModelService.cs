using System;
using System.Linq;
using System.Threading.Tasks;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Web.Interfaces.GitHub;
using MoreLinq.Extensions;
using Octokit;
using CheckRun = BCC.Web.Models.GitHub.CheckRun;

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
        public async Task<CheckRun> SubmitCheckRunAsync(string owner, string repository, string sha, CreateCheckRun createCheckRun, Annotation[] annotations)
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

            if (string.IsNullOrWhiteSpace(createCheckRun.Name))
            {
                throw new ArgumentException("Name is invalid", nameof(createCheckRun.Name));
            }

            if (string.IsNullOrWhiteSpace(createCheckRun.Title))
            {
                throw new ArgumentException("Title is invalid", nameof(createCheckRun.Title));
            }

            var annotationBatches = annotations?.Batch(50).ToArray();
            var checkRun = await CreateCheckRunAsync(owner, repository, sha, createCheckRun, annotationBatches?.FirstOrDefault()?.ToArray())
                .ConfigureAwait(false);

            if (annotationBatches != null)
            {
                foreach (var annotationBatch in annotationBatches.Skip(1))
                {
                    await UpdateCheckRunAsync(checkRun.Id, owner, repository, createCheckRun, annotationBatch.ToArray()).ConfigureAwait(false);
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
        /// <param name="createCheckRun"></param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <param name="name">The name of the CheckRun.</param>
        /// <param name="title">The title of the CheckRun.</param>
        /// <param name="summary">The summary of the CheckRun.</param>
        /// <param name="success">If the CheckRun is a success.</param>
        /// <param name="startedAt">The time when processing started</param>
        /// <param name="completedAt">The time when processing finished</param>
        /// <returns></returns>
        public async Task<CheckRun> CreateCheckRunAsync(string owner, string repository, string sha,
            CreateCheckRun createCheckRun, Annotation[] annotations)
        {
            try
            {
                if (owner == null) throw new ArgumentNullException(nameof(owner));
                if (repository == null) throw new ArgumentNullException(nameof(repository));
                if (sha == null) throw new ArgumentNullException(nameof(sha));

                if ((annotations?.Length ?? 0) > 50)
                    throw new ArgumentException("Cannot create more than 50 annotations at a time");

                var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
                var checkRunsClient = gitHubClient?.Check?.Run;

                if (checkRunsClient == null) throw new InvalidOperationException("ICheckRunsClient is null");

                var newCheckRun = new NewCheckRun(createCheckRun.Name, sha)
                {
                    Output = new NewCheckRunOutput(createCheckRun.Title, createCheckRun.Summary)
                    {
                        Text = createCheckRun.Text,
                        Images = createCheckRun.Images?.Select(image => new NewCheckRunImage(image.Alt, image.ImageUrl){ Caption =  image.Caption}).ToArray(),
                        Annotations = annotations?
                            .Select(CreateNewCheckRunAnnotation)
                            .ToArray()
                    },
                    Status = CheckStatus.Completed,
                    StartedAt = createCheckRun.StartedAt,
                    CompletedAt = createCheckRun.CompletedAt,
                    Conclusion = createCheckRun.Conclusion.ToOctokit()
                };

                var checkRun = await checkRunsClient.Create(owner, repository, newCheckRun);

                return new CheckRun
                {
                    Id = checkRun.Id,
                    Url = checkRun.HtmlUrl
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
        /// <param name="repository">The name of the repository.</param>
        /// <param name="createCheckRun">The CheckRun details.</param>
        /// <param name="annotations">Array of Annotations for the CheckRun.</param>
        /// <returns></returns>
        public async Task UpdateCheckRunAsync(long checkRunId, string owner, string repository,
            CreateCheckRun createCheckRun, Annotation[] annotations)
        {
            try
            {
                if (annotations.Length > 50)
                    throw new ArgumentException("Cannot create more than 50 annotations at a time");

                var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
                var checkRunsClient = gitHubClient?.Check?.Run;

                if (checkRunsClient == null) throw new InvalidOperationException("ICheckRunsClient is null");

                await checkRunsClient.Update(owner, repository, checkRunId, new CheckRunUpdate
                {
                    Output = new NewCheckRunOutput(createCheckRun.Title, createCheckRun.Summary)
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

        public async Task<string[]> GetPullRequestFiles(string owner, string repository, int number)
        {
            var gitHubClient = await _gitHubAppClientFactory.CreateAppClientForLoginAsync(_tokenGenerator, owner);
            var pullRequestsClient = gitHubClient?.Repository.PullRequest;

            if (pullRequestsClient == null) throw new InvalidOperationException("IPullRequestsClient is null");
            var files = await pullRequestsClient.Files(owner, repository, number);
            return files.Select(file => file.FileName).ToArray();
        }

        private static NewCheckRunAnnotation CreateNewCheckRunAnnotation(Annotation annotation)
        {
            var newCheckRunAnnotation = new NewCheckRunAnnotation(annotation.Filename,
                annotation.StartLine, annotation.EndLine, GetCheckWarningLevel(annotation),
                annotation.Message);

            if (!string.IsNullOrWhiteSpace(annotation.Title))
            {
                newCheckRunAnnotation.Title = annotation.Title;
            }

            return newCheckRunAnnotation;
        }

        private static CheckAnnotationLevel GetCheckWarningLevel(Annotation annotation)
        {
            switch (annotation.AnnotationLevel)
            {
                case AnnotationLevel.Notice:
                    return CheckAnnotationLevel.Notice;
                case AnnotationLevel.Warning:
                    return CheckAnnotationLevel.Warning;
                case AnnotationLevel.Failure:
                    return CheckAnnotationLevel.Failure;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}