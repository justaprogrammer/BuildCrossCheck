using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Interfaces.GitHub;
using MSBLOC.Core.Model.CheckRunSubmission;
using MSBLOC.Core.Model.GitHub;
using Newtonsoft.Json;

namespace MSBLOC.Core.Services.CheckRunSubmission
{
    /// <inheritdoc/>
    public class CheckRunSubmissionService : ICheckRunSubmissionService
    {
        private readonly ILogger<CheckRunSubmissionService> _logger;
        private readonly IFileSystem _fileSystem;
        private readonly IGitHubAppModelService _gitHubAppModelService;

        public CheckRunSubmissionService(ILogger<CheckRunSubmissionService> logger, IFileSystem fileSystem,
            IGitHubAppModelService gitHubAppModelService)
        {
            _logger = logger;
            _fileSystem = fileSystem;
            _gitHubAppModelService = gitHubAppModelService;
        }

        /// <inheritdoc/>
        public Task<CheckRun> SubmitAsync([NotNull] string owner, [NotNull] string repository, [NotNull] string sha, [NotNull] string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("Owner is null", nameof(owner));
            }

            if (string.IsNullOrWhiteSpace(repository))
            {
                throw new ArgumentException("Repository is null", nameof(repository));
            }

            if (string.IsNullOrWhiteSpace(sha))
            {
                throw new ArgumentException("Sha is null", nameof(sha));
            }

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                throw new ArgumentException("ResourcePath is null", nameof(resourcePath));
            }

            _logger.LogInformation("SubmitAsync owner:{0} repository:{1} sha:{2} resourcePath:{3}",
                owner, repository, sha, resourcePath);

            var readAllText = _fileSystem.File.ReadAllText(resourcePath);

            var createCheckRun = JsonConvert.DeserializeObject<CreateCheckRun>(readAllText);

            return _gitHubAppModelService.SubmitCheckRun(owner, repository, sha, createCheckRun.Name, createCheckRun.Title,
                createCheckRun.Summary, createCheckRun.Success, createCheckRun.Annotations,
                createCheckRun.StartedAt, createCheckRun.CompletedAt);
        }
    }
}