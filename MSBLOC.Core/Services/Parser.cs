extern alias StructuredLogger;
using System.Collections;
using System.Linq;
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

        public BuildDetails Parse(string resourcePath, string cloneRoot)
        {
            var binLogReader = new StructuredLogger::Microsoft.Build.Logging.BinaryLogReplayEventSource();

            var solutionDetails = new SolutionDetails(cloneRoot);
            var buildDetails = new BuildDetails(solutionDetails);

            foreach (var record in binLogReader.ReadRecords(resourcePath))
            {
                var buildEventArgs = record.Args;
                if (buildEventArgs is ProjectStartedEventArgs startedEventArgs)
                {
                    var projectDetails = new ProjectDetails(cloneRoot, startedEventArgs.ProjectFile);
                    solutionDetails.Add(projectDetails);

                    var items = startedEventArgs.Items.Cast<DictionaryEntry>()
                        .Where(entry => (string) entry.Key == "Compile")
                        .Select(entry => entry.Value)
                        .Cast<ITaskItem>()
                        .Select(item => item.ItemSpec)
                        .ToArray();

                    projectDetails.AddItems(items);
                }

                if (buildEventArgs is BuildWarningEventArgs buildWarning)
                {
                    var endLine = buildWarning.EndLineNumber;
                    if (endLine == 0)
                    {
                        endLine = buildWarning.LineNumber;
                    }

                    var projectItemPath = solutionDetails.GetProjectItemPath(buildWarning.ProjectFile, buildWarning.File);
                    buildDetails.AddAnnotation(projectItemPath, buildWarning.LineNumber, endLine, AnnotationWarningLevel.Warning, buildWarning.Message, buildWarning.Code);
                }

                if (buildEventArgs is BuildErrorEventArgs buildError)
                {
                    var endLine = buildError.EndLineNumber;
                    if (endLine == 0)
                    {
                        endLine = buildError.LineNumber;
                    }

                    var projectItemPath = solutionDetails.GetProjectItemPath(buildError.ProjectFile, buildError.File);
                    buildDetails.AddAnnotation(projectItemPath, buildError.LineNumber, endLine, AnnotationWarningLevel.Failure, buildError.Message, buildError.Code);
                }
            }

            return buildDetails;
        }
    }
}