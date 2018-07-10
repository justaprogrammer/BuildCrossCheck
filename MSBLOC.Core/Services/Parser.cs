using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MSBLOC.Core.Services
{
    public class Parser
    {
        private ILogger<Parser> Logger { get; }

        public Parser(ILogger<Parser> logger = null)
        {
            Logger = logger ?? new NullLogger<Parser>();
        }

        public void Parse(string resourcePath)
        {
            var binLogReader = new BinaryLogReplayEventSource();
            foreach (var record in binLogReader.ReadRecords(resourcePath))
            {
                var buildEventArgs = record.Args;
                if (buildEventArgs is BuildWarningEventArgs buildWarning)
                {
                    Logger.LogInformation($"{buildWarning.File} {buildWarning.LineNumber}:{buildWarning.ColumnNumber} {buildWarning.Message}");
                }

                if (buildEventArgs is BuildErrorEventArgs buildError)
                {
                    Logger.LogInformation($"{buildError.File} {buildError.LineNumber}:{buildError.ColumnNumber} {buildError.Message}");
                }
            }
        }
    }
}
