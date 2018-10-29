using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using BCC.Core.Interfaces;
using BCC.Core.Interfaces.GitHub;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Model.GitHub;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CheckRun = BCC.Web.Models.GitHub.CheckRun;
using ICheckRunSubmissionService = BCC.Web.Interfaces.ICheckRunSubmissionService;
using IGitHubAppModelService = BCC.Web.Interfaces.GitHub.IGitHubAppModelService;

namespace BCC.Web.Services
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

            return _gitHubAppModelService.SubmitCheckRunAsync(owner, repository, sha, createCheckRun.Name, createCheckRun.Title,
                createCheckRun.Summary, createCheckRun.Success, createCheckRun.Annotations,
                createCheckRun.StartedAt, createCheckRun.CompletedAt);
        }
    }
}