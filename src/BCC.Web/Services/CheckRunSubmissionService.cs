using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BCC.Core.Model.CheckRunSubmission;
using BCC.Core.Serialization;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Collections;
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
        public async Task<CheckRun> SubmitAsync([NotNull] string owner, [NotNull] string repository, [NotNull] string sha,
            [NotNull] string resourcePath, int pullRequestNumber)
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

            var readAllText = _fileSystem.File.ReadAllText(resourcePath, Encoding.Unicode);

            var createCheckRun = CreateCheckRunSerializer.DeSerialize(readAllText);

            if (createCheckRun.Summary != null)
            {
                var byteCount = Encoding.Unicode.GetByteCount(createCheckRun.Summary) / 1024.0;
                if (byteCount > 128.0)
                {
                    throw new InvalidOperationException();
                }
            }

            var pullRequestFiles = await _gitHubAppModelService.GetPullRequestFiles(owner, repository, pullRequestNumber);
            var hashSet = new HashSet(pullRequestFiles);

            var annotations = createCheckRun
                .Annotations.Where(annotation => hashSet.Contains(annotation.Filename))
                .ToArray();

            return await _gitHubAppModelService.SubmitCheckRunAsync(owner, repository, sha, createCheckRun, annotations);
        }
    }
}