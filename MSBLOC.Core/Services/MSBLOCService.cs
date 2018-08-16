using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MoreLinq;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;

namespace MSBLOC.Core.Services
{
    public class MSBLOCService : IMSBLOCService
    {
        private readonly IBinaryLogProcessor _binaryLogProcessor;
        private readonly IGitHubAppModelService _gitHubAppModelService;
        private readonly ILogger _logger;

        public MSBLOCService(
            IBinaryLogProcessor binaryLogProcessor,
            IGitHubAppModelService gitHubAppModelService,
            ILogger<MSBLOCService> logger)
        {
            _logger = logger;
            _binaryLogProcessor = binaryLogProcessor;
            _gitHubAppModelService = gitHubAppModelService;
        }

        public async Task<CheckRun> SubmitAsync(string repoOwner, string repoName, string sha, string cloneRoot,
            string resourcePath)
        {
            var startedAt = DateTimeOffset.Now;

            var buildDetails = _binaryLogProcessor.ProcessLog(resourcePath, cloneRoot, repoOwner, repoName, sha);

            var checkRun = await SubmitCheckRun(buildDetails: buildDetails,
                owner: repoOwner,
                name: repoName,
                headSha: sha,
                checkRunName: "MSBuildLog Analyzer",
                checkRunTitle: "MSBuildLog Analysis",
                checkRunSummary: "",
                startedAt: startedAt,
                completedAt: DateTimeOffset.Now).ConfigureAwait(false);

            _logger.LogInformation($"CheckRun Created - {checkRun.Url}");

            return checkRun;
        }

        protected async Task<CheckRun> SubmitCheckRun(BuildDetails buildDetails,
            string owner, string name, string headSha,
            string checkRunName, string checkRunTitle, string checkRunSummary,
            DateTimeOffset startedAt, DateTimeOffset completedAt)
        {
            var annotations = buildDetails.Annotations?.Batch(50).ToArray();

            var checkRun = await _gitHubAppModelService.CreateCheckRunAsync(owner, name, headSha, checkRunName, checkRunTitle, checkRunSummary, annotations?.FirstOrDefault()?.ToArray(), startedAt, completedAt).ConfigureAwait(false);

            foreach (var annotationBatch in annotations.Skip(1))
            {
                await _gitHubAppModelService.UpdateCheckRunAsync(checkRun.Id, owner, name, headSha, checkRunTitle, checkRunSummary,
                    annotationBatch.ToArray(), startedAt, completedAt).ConfigureAwait(false);
            }

            return checkRun;
        }
    }
}
