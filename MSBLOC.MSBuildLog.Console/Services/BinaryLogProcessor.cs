extern alias StructuredLogger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MoreLinq;
using MSBLOC.Core.Model.CheckRunSubmission;
using MSBLOC.Core.Model.LogAnalyzer;
using MSBLOC.MSBuildLog.Console.Extensions;
using MSBLOC.MSBuildLog.Console.Interfaces;

namespace MSBLOC.MSBuildLog.Console.Services
{
    public class BinaryLogProcessor : IBinaryLogProcessor
    {
        private ILogger<BinaryLogProcessor> Logger { get; }

        public BinaryLogProcessor(ILogger<BinaryLogProcessor> logger = null)
        {
            Logger = logger ?? new NullLogger<BinaryLogProcessor>();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Annotation> ProcessLog(string binLogPath, string cloneRoot)
        {
            Logger.LogInformation("ProcessLog binLogPath:{0} cloneRoot:{1}", binLogPath, cloneRoot);

            var binLogReader = new StructuredLogger::Microsoft.Build.Logging.BinaryLogReplayEventSource();

            var annotations = new List<Annotation>();

            foreach (var record in binLogReader.ReadRecords(binLogPath))
            {
                var buildEventArgs = record.Args;
                if (buildEventArgs is BuildWarningEventArgs buildWarning)
                {
                    var annotation = CreateAnnotation(CheckWarningLevel.Warning,
                        cloneRoot, 
                        buildWarning.ProjectFile, 
                        buildWarning.File, 
                        buildWarning.Code,
                        buildWarning.Message,
                        buildWarning.LineNumber,
                        buildWarning.EndLineNumber);

                    annotations.Add(annotation);
                }

                if (buildEventArgs is BuildErrorEventArgs buildError)
                {
                    var annotation = CreateAnnotation(CheckWarningLevel.Failure,
                        cloneRoot, 
                        buildError.ProjectFile, 
                        buildError.File, 
                        buildError.Code, 
                        buildError.Message,
                        buildError.LineNumber, 
                        buildError.EndLineNumber);

                    annotations.Add(annotation);
                }
            }

            return annotations.ToArray();
        }

        private Annotation CreateAnnotation(CheckWarningLevel checkWarningLevel, string cloneRoot, string projectFile,
            string file, string title, string message, int lineNumber, int endLineNumber)
        {
            var filePath = Path.Combine(Path.GetDirectoryName(projectFile), file);
            if (!filePath.IsSubPathOf(cloneRoot))
            {
                throw new InvalidOperationException($"FilePath `{filePath}` is not a child of `{cloneRoot}`");
            }

            var relativePath = GetRelativePath(filePath, cloneRoot);

            return new Annotation(relativePath, checkWarningLevel,
                title, message,
                lineNumber,
                endLineNumber == 0 ? lineNumber : endLineNumber);
        }

        private string GetRelativePath(string filespec, string folder)
        {
            //https://stackoverflow.com/a/703292/104877

            var pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }

            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
