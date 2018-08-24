extern alias StructuredLogger;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoreLinq.Extensions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model.Builds;

namespace MSBLOC.Core.Services
{
    public class BinaryLogProcessor : IBinaryLogProcessor
    {
        private ILogger<BinaryLogProcessor> Logger { get; }

        public BinaryLogProcessor(ILogger<BinaryLogProcessor> logger = null)
        {
            Logger = logger ?? new NullLogger<BinaryLogProcessor>();
        }

        /// <inheritdoc />
        public BuildDetails ProcessLog(string binLogPath, string cloneRoot)
        {
            var binLogReader = new StructuredLogger::Microsoft.Build.Logging.BinaryLogReplayEventSource();

            var solutionDetails = new SolutionDetails(cloneRoot);
            var buildDetails = new BuildDetails(solutionDetails);
            var buildMessages = new List<BuildMessage>();

            foreach (var record in binLogReader.ReadRecords(binLogPath))
            {
                var buildEventArgs = record.Args;
                if (buildEventArgs is ProjectStartedEventArgs startedEventArgs)
                {
                    var notPresent = !solutionDetails.ContainsKey(startedEventArgs.ProjectFile);

                    var items = startedEventArgs.Items?.Cast<DictionaryEntry>()
                        .Where(entry => (string)entry.Key == "Compile")
                        .Select(entry => entry.Value)
                        .Cast<ITaskItem>()
                        .Select(item => item.ItemSpec)
                        .ToArray();

                    if (notPresent && (items?.Any() ?? false))
                    {
                        var projectDetails = new ProjectDetails(cloneRoot, startedEventArgs.ProjectFile);
                        solutionDetails.Add(projectDetails);

                        if (items != null)
                        {
                            projectDetails.AddItems(items);
                        }
                    }
                }

                BuildMessage buildMessage = null;
                if (buildEventArgs is BuildWarningEventArgs buildWarning)
                {
                    buildMessage = CreateBuildMessage(
                        BuildMessageLevel.Warning,
                        buildWarning.ProjectFile,
                        buildWarning.File,
                        buildWarning.LineNumber,
                        buildWarning.EndLineNumber == 0 ? buildWarning.LineNumber : buildWarning.EndLineNumber,
                        buildWarning.Message,
                        buildWarning.Code,
                        buildDetails);
                }

                if (buildEventArgs is BuildErrorEventArgs buildError)
                {
                    buildMessage = CreateBuildMessage(
                        BuildMessageLevel.Error,
                        buildError.ProjectFile,
                        buildError.File,
                        buildError.LineNumber,
                        buildError.EndLineNumber == 0 ? buildError.LineNumber : buildError.EndLineNumber,
                        buildError.Message,
                        buildError.Code,
                        buildDetails);
                }

                if (buildMessage != null)
                {
                    buildMessages.Add(buildMessage);
                }
            }

            var distinctBy = buildMessages
                .DistinctBy(message => (message.ProjectFile, message.MessageLevel, message.File, message.Code, message.Message, message.LineNumber, message.EndLineNumber))
                .ToArray();

            buildDetails.AddMessages(distinctBy);

            return buildDetails;
        }

        private static BuildMessage CreateBuildMessage(BuildMessageLevel buildMessageLevel, string projectFile,
            string file, int lineNumber, int endLineNumber, string message, string code, BuildDetails buildDetails)
        {
            if (code.StartsWith("CA"))
            {
                var projectDetails = buildDetails.SolutionDetails[projectFile];
                file = file.Substring(projectDetails.ProjectDirectory.Length + 1);
            }

            return new BuildMessage(
                buildMessageLevel,
                projectFile,
                file,
                lineNumber,
                endLineNumber,
                message,
                code);
        }
    }
}
