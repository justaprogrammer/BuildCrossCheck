extern alias StructuredLogger;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;

namespace MSBLOC.Core.Services
{
    public class Parser : IParser
    {
        private ILogger<Parser> Logger { get; }

        public Parser(ILogger<Parser> logger = null)
        {
            Logger = logger ?? new NullLogger<Parser>();
        }

        public ParsedBinaryLog Parse(string resourcePath)
        {
            var warnings = new List<BuildWarningEventArgs>();
            var errors = new List<BuildErrorEventArgs>();
            var binLogReader = new StructuredLogger::Microsoft.Build.Logging.BinaryLogReplayEventSource();
            foreach (var record in binLogReader.ReadRecords(resourcePath))
            {
                var buildEventArgs = record.Args;
                if (buildEventArgs is BuildWarningEventArgs buildWarning)
                {
                    Logger.LogInformation($"{buildWarning.File} {buildWarning.LineNumber}:{buildWarning.ColumnNumber} {buildWarning.Message}");
                    warnings.Add(buildWarning);
                }

                if (buildEventArgs is BuildErrorEventArgs buildError)
                {
                    Logger.LogInformation($"{buildError.File} {buildError.LineNumber}:{buildError.ColumnNumber} {buildError.Message}");
                    errors.Add(buildError);
                }
            }

            return new ParsedBinaryLog(warnings.ToArray(), errors.ToArray());
        }
    }
}
