using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class Submitter : ISubmitter
    {
        private ICheckRunsClient CheckRunsClient { get; }
        private ILogger<Submitter> Logger { get; }

        public Submitter(ICheckRunsClient checkRunsClient, ILogger<Submitter> logger = null)
        {
            CheckRunsClient = checkRunsClient;
            Logger = logger ?? new NullLogger<Submitter>();
        }

        public async Task<CheckRun> SubmitCheckRun(string owner, string name, string headSha,
            string checkRunName, BuildDetails buildDetails, string checkRunTitle, string checkRunSummary, string cloneRoot)
        {
            var newCheckRunAnnotations = buildDetails.Annotations.Select(annotation =>
            {
                var warningLevel = GetCheckWarningLevel(annotation.AnnotationWarningLevel);
                var newCheckRunAnnotation = new NewCheckRunAnnotation(annotation.Filename, "", annotation.LineNumber, annotation.EndLine, warningLevel, annotation.Message)
                {
                    Title = annotation.Title
                };

                return newCheckRunAnnotation;
            }).ToList();

            var newCheckRun = new NewCheckRun(checkRunName, headSha)
            {
                Output = new NewCheckRunOutput(checkRunTitle, checkRunSummary)
                {
                    Annotations = newCheckRunAnnotations
                }
            };

            return await CheckRunsClient.Create(owner, name, newCheckRun);
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