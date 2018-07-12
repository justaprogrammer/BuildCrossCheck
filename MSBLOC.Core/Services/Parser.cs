using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using Octokit;

namespace MSBLOC.Core.Services
{
    public class Parser : IParser
    {
        private ILogger<Parser> Logger { get; }

        public Parser(ILogger<Parser> logger = null)
        {
            Logger = logger ?? new NullLogger<Parser>();
        }

        public CheckRunAnnotation[] Parse(string resourcePath)
        {
            var annotations = new List<CheckRunAnnotation>();
            var binLogReader = new BinaryLogReplayEventSource();
            foreach (var record in binLogReader.ReadRecords(resourcePath))
            {
                var buildEventArgs = record.Args;
                if (buildEventArgs is BuildWarningEventArgs buildWarning)
                {
                    Logger.LogInformation($"{buildWarning.File} {buildWarning.LineNumber}:{buildWarning.ColumnNumber} {buildWarning.Message}");
                    var endLine = buildWarning.EndLineNumber;
                    if (endLine == 0)
                    {
                        endLine = buildWarning.LineNumber;
                    }

                    annotations.Add(CreateAnnotation(buildWarning.File, "", buildWarning.LineNumber, endLine, CheckWarningLevel.Warning, buildWarning.Code, buildWarning.Message, string.Empty));
                }

                if (buildEventArgs is BuildErrorEventArgs buildError)
                {
                    Logger.LogInformation($"{buildError.File} {buildError.LineNumber}:{buildError.ColumnNumber} {buildError.Message}");
                    var endLine = buildError.EndLineNumber;
                    if (endLine == 0)
                    {
                        endLine = buildError.LineNumber;
                    }

                    annotations.Add(CreateAnnotation(buildError.File, "", buildError.LineNumber, endLine, CheckWarningLevel.Failure, buildError.Code, buildError.Message, string.Empty));
                }
            }

            return annotations.ToArray();
        }

        private static CheckRunAnnotation CreateAnnotation(string fileName, string blobHref, int startLine, int endLine,
            CheckWarningLevel checkWarningLevel, string title, string message, string rawDetails)
        {
            return new CheckRunAnnotation(fileName, blobHref, startLine, endLine, checkWarningLevel, message)
            {
                Title = title,
                RawDetails = rawDetails
            };
        }
    }
}
