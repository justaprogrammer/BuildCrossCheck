using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoreLinq.Extensions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class CheckRunSubmitter : ICheckRunSubmitter
    {
        /// <remarks>We can only send 50 annotations per rest call.</remarks>
        private const int AnnotationBatchSize = 50;

        private ICheckRunsClient CheckRunsClient { get; }
        private ILogger<CheckRunSubmitter> Logger { get; }

        public CheckRunSubmitter(ICheckRunsClient checkRunsClient,
            ILogger<CheckRunSubmitter> logger = null)
        {
            CheckRunsClient = checkRunsClient;
            Logger = logger ?? new NullLogger<CheckRunSubmitter>();
        }

        /// <inheritdoc />
        public async Task<CheckRun> SubmitCheckRun(BuildDetails buildDetails,
            string owner, string name, string headSha,
            string checkRunName, string checkRunTitle, string checkRunSummary,
            DateTimeOffset startedAt, DateTimeOffset completedAt)
        {
            using (Logger.BeginScope(new
            {
                Owner = owner,
                Name = name,
                HeadSha = headSha,
                CheckRunName = checkRunName
            }))
            {
                var newCheckRunAnnotations = buildDetails.Annotations.Select(annotation =>
                {
                    var warningLevel = GetCheckWarningLevel(annotation.AnnotationWarningLevel);
                    var blobHref = BlobHref(owner, name, headSha, annotation.Filename);
                    var newCheckRunAnnotation = new NewCheckRunAnnotation(annotation.Filename, blobHref,
                        annotation.LineNumber, annotation.EndLine, warningLevel, annotation.Message)
                    {
                        Title = annotation.Title
                    };

                    return newCheckRunAnnotation;
                }).Batch(AnnotationBatchSize).ToArray();

                Logger.LogDebug("BuildDetails contains {0} annotations. Will be broken up in {1} batches of {2}",
                    buildDetails.Annotations.Count, newCheckRunAnnotations.Length, AnnotationBatchSize);

                var checkConclusion = buildDetails.Annotations
                    .Any(annotation => annotation.AnnotationWarningLevel == AnnotationWarningLevel.Failure)
                    ? CheckConclusion.Failure
                    : CheckConclusion.Success;

                Logger.LogTrace("BuildDetails Conclusion {0}", checkConclusion);

                var newCheckRun = new NewCheckRun(checkRunName, headSha)
                {
                    Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                    {
                        Annotations = newCheckRunAnnotations.FirstOrDefault()?.ToArray()
                    },
                    Status = CheckStatus.Completed,
                    StartedAt = startedAt,
                    CompletedAt = completedAt,
                    Conclusion = checkConclusion
                };

                var checkRun = await CheckRunsClient.Create(owner, name, newCheckRun);

                Logger.LogTrace("CheckRun Created with {0} annotations.", newCheckRun.Output?.Annotations?.Count());

                foreach (var newCheckRunAnnotation in newCheckRunAnnotations.Skip(1))
                {
                    var annotations = newCheckRunAnnotation.ToArray();
                    await CheckRunsClient.Update(owner, name, checkRun.Id, new CheckRunUpdate()
                    {
                        Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                        {
                            Annotations = annotations
                        },
                        Status = CheckStatus.Completed,
                        StartedAt = startedAt,
                        CompletedAt = completedAt,
                        Conclusion = checkConclusion
                    });
                    Logger.LogTrace("{0} annotation added to CheckRun.", annotations.Count());
                }

                Logger.LogInformation("CheckRun submitted sucessfully");

                return checkRun;
            }
        }

        private static string BlobHref(string owner, string repository, string sha, string file)
        {
            return $"https://github.com/{owner}/{repository}/blob/{sha}/{file.Replace(@"\", "/")}";
        }
        
        private static CheckWarningLevel GetCheckWarningLevel(AnnotationWarningLevel annotationWarningLevel)
        {
            CheckWarningLevel warningLevel;
            switch (annotationWarningLevel)
            {
                case AnnotationWarningLevel.Warning:
                    warningLevel = CheckWarningLevel.Warning;
                    break;
                case AnnotationWarningLevel.Notice:
                    warningLevel = CheckWarningLevel.Notice;
                    break;
                case AnnotationWarningLevel.Failure:
                    warningLevel = CheckWarningLevel.Failure;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(annotationWarningLevel));
            }

            return warningLevel;
        }
    }
}