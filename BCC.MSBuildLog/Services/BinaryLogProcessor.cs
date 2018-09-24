extern alias StructuredLogger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Extensions;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BCC.MSBuildLog.Services
{
    public class BinaryLogProcessor : IBinaryLogProcessor
    {
        private readonly IBinaryLogReader _binaryLogReader;
        private ILogger<BinaryLogProcessor> Logger { get; }

        public BinaryLogProcessor(IBinaryLogReader binaryLogReader, ILogger<BinaryLogProcessor> logger = null)
        {
            _binaryLogReader = binaryLogReader;
            Logger = logger ?? new NullLogger<BinaryLogProcessor>();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<Annotation> CreateAnnotations(string binLogPath, string cloneRoot, CheckRunConfiguration configuration = null)
        {
            Logger.LogInformation("CreateAnnotations binLogPath:{0} cloneRoot:{1}", binLogPath, cloneRoot);

            var ruleDictionary = 
                configuration?.Rules?.ToDictionary(rule => rule.Code, rule => rule.ReportAs);

            var annotations = new List<Annotation>();
            foreach (var record in _binaryLogReader.ReadRecords(binLogPath))
            {
                var buildEventArgs = record.Args;

                var buildWarning = buildEventArgs as BuildWarningEventArgs;
                var buildError = buildEventArgs as BuildErrorEventArgs;

                if (buildWarning == null && buildError == null)
                    continue;

                var checkWarningLevel = (buildWarning != null) ? CheckWarningLevel.Warning : CheckWarningLevel.Failure;
                var buildCode = buildWarning?.Code ?? buildError.Code;
                var projectFile = buildWarning?.ProjectFile ?? buildError.ProjectFile;
                var file = buildWarning?.File ?? buildError.File;
                var title = buildWarning?.Code ?? buildError.Code;
                var message = buildWarning?.Message ?? buildError.Message;
                var lineNumber = buildWarning?.LineNumber ?? buildError.LineNumber;
                var endLineNumber = buildWarning?.EndLineNumber ?? buildError.EndLineNumber;

                ReportAs reportAs = ReportAs.AsIs;
                if (ruleDictionary?.TryGetValue(buildCode, out reportAs) ?? false)
                {
                    switch (reportAs)
                    {
                        case ReportAs.Ignore:
                            continue;

                        case ReportAs.AsIs:
                            break;

                        case ReportAs.Notice:
                            checkWarningLevel = CheckWarningLevel.Notice;
                            break;
                        case ReportAs.Warning:
                            checkWarningLevel = CheckWarningLevel.Warning;
                            break;
                        case ReportAs.Error:
                            checkWarningLevel = CheckWarningLevel.Failure;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                annotations.Add(CreateAnnotation(checkWarningLevel,
                    cloneRoot,
                    projectFile,
                    file,
                    title,
                    message,
                    lineNumber,
                    endLineNumber));
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

            var relativePath = GetRelativePath(filePath, cloneRoot).Replace("\\", "/");

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
