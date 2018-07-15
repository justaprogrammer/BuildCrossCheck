extern alias StructuredLogger;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Models;


namespace MSBLOC.Core.Services
{
    public class Parser : IParser
    {
        private ILogger<Parser> Logger { get; }

        public Parser(ILogger<Parser> logger = null)
        {
            Logger = logger ?? new NullLogger<Parser>();
        }

        public StubAnnotation[] Parse(string resourcePath)
        {
            var stubAnnotations = new List<StubAnnotation>();
            var binLogReader = new StructuredLogger::Microsoft.Build.Logging.BinaryLogReplayEventSource();
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

                    stubAnnotations.Add(new StubAnnotation
                    {
                        FileName = buildWarning.File,
                        Title = buildWarning.Code,
                        Message = buildWarning.Message,
                        WarningLevel = "Warning",
                        StartLine = buildWarning.LineNumber,
                        EndLine = endLine
                    });
                }

                if (buildEventArgs is BuildErrorEventArgs buildError)
                {
                    Logger.LogInformation($"{buildError.File} {buildError.LineNumber}:{buildError.ColumnNumber} {buildError.Message}");
                    var endLine = buildError.EndLineNumber;
                    if (endLine == 0)
                    {
                        endLine = buildError.LineNumber;
                    }

                    stubAnnotations.Add(new StubAnnotation
                    {
                        FileName = buildError.File,
                        Title = buildError.Code,
                        Message = buildError.Message,
                        WarningLevel = "Error",
                        StartLine = buildError.LineNumber,
                        EndLine = endLine
                    });
                }
            }

            return stubAnnotations.ToArray();
        }
    }
}
