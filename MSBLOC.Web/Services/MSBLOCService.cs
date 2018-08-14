using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitHubJwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using MSBLOC.Core.Services;
using MSBLOC.Web.Interfaces;
using MSBLOC.Web.Models;

namespace MSBLOC.Web.Services
{
    public class MSBLOCService : IMSBLOCService
    {
        private readonly IBinaryLogProcessor _binaryLogProcessor;
        private readonly IGitHubAppModelService _gitHubAppModelService;
        private readonly ITempFileService _tempFileService;
        private readonly ILogger _logger;

        public MSBLOCService(
            IBinaryLogProcessor binaryLogProcessor,
            IGitHubAppModelService gitHubAppModelService, 
            ITempFileService tempFileService,
            ILogger<MSBLOCService> logger)
        {
            _logger = logger;
            _binaryLogProcessor = binaryLogProcessor;
            _gitHubAppModelService = gitHubAppModelService;
            _tempFileService = tempFileService;
        }

        public async Task<CheckRun> SubmitAsync(SubmissionData submissionData)
        {
            var repoOwner = submissionData.RepoOwner;
            var repoName = submissionData.RepoName;
            var cloneRoot = submissionData.CloneRoot;
            var sha = submissionData.CommitSha;

            var resourcePath = _tempFileService.GetFilePath(submissionData.BinaryLogFile);

            var startedAt = DateTimeOffset.Now;

            var buildDetails = _binaryLogProcessor.ProcessLog(resourcePath, cloneRoot);

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

            var checkRun = await _gitHubAppModelService.CreateCheckRun(owner, name, headSha, checkRunName, checkRunTitle, checkRunSummary, annotations?.FirstOrDefault()?.ToArray(), startedAt, completedAt).ConfigureAwait(false);

            foreach (var annotationBatch in annotations.Skip(1))
            {
                await _gitHubAppModelService.UpdateCheckRun(checkRun.Id, owner, name, headSha, checkRunTitle, checkRunSummary,
                    annotationBatch.ToArray(), startedAt, completedAt).ConfigureAwait(false);
            }

            return checkRun;
        }
    }
}
