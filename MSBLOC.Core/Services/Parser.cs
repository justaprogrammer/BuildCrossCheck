extern alias StructuredLogger;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using MSBLOC.Core.Interfaces;
using MSBLOC.Core.Model;

namespace MSBLOC.Core.Services
{
    public class SolutionDetails
    {
        private Dictionary<string, ProjectDetails> Projects { get; }
        private string CloneRoot { get; }

        public SolutionDetails(string cloneRoot)
        {
            CloneRoot = cloneRoot;
            Projects = new Dictionary<string, ProjectDetails>();
        }

        public void AddProject(ProjectDetails projectDetails)
        {
            Projects.Add(projectDetails.ProjectFile, projectDetails);
        }

        public ProjectDetails GetProject(string projectFile)
        {
            return Projects[projectFile];
        }
    }

    public class ProjectDetails
    {
        public string ProjectFile { get; }
        private string CloneRoot { get; }
        private string ProjectDirectory { get; }

        private Dictionary<string, string> _taskItemPaths;

        public ProjectDetails(string cloneRoot, string projectFile)
        {
            CloneRoot = cloneRoot;
            ProjectFile = projectFile;

            ProjectDirectory = Path.GetDirectoryName(projectFile)
                                   ?? throw new InvalidOperationException("Path.GetDirectoryName(startedEventArgs.ProjectFile) is null");

            _taskItemPaths = new Dictionary<string, string>();
        }

        public void AddTaskItems(IEnumerable<ITaskItem> taskItems)
        {
            _taskItemPaths = taskItems
                .ToDictionary(item => item.ItemSpec, item => Path.Combine(ProjectDirectory, item.ItemSpec));
        }

        public string GetClonePath(string taskItems)
        {
            return _taskItemPaths[taskItems];
        }
    }

    public class Parser : IParser
    {
        private ILogger<Parser> Logger { get; }

        public Parser(ILogger<Parser> logger = null)
        {
            Logger = logger ?? new NullLogger<Parser>();
        }

        public ParsedBinaryLog Parse(string resourcePath, string cloneRoot = "")
        {
            var warnings = new List<BuildWarningEventArgs>();
            var errors = new List<BuildErrorEventArgs>();
            var binLogReader = new StructuredLogger::Microsoft.Build.Logging.BinaryLogReplayEventSource();

            var readRecords = binLogReader.ReadRecords(resourcePath).ToList();

            var solutionDetails = new SolutionDetails(cloneRoot);

            foreach (var record in readRecords)
            {
                var buildEventArgs = record.Args;
                if (buildEventArgs is ProjectStartedEventArgs startedEventArgs)
                {
                    var projectDetails = new ProjectDetails(cloneRoot, startedEventArgs.ProjectFile);
                    solutionDetails.AddProject(projectDetails);

                    var taskItems = startedEventArgs.Items.Cast<DictionaryEntry>()
                        .Where(entry => (string)entry.Key == "Compile")
                        .Select(entry => entry.Value)
                        .Cast<ITaskItem>();

                    projectDetails.AddTaskItems(taskItems);
                }

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

            return new ParsedBinaryLog(warnings.ToArray(), errors.ToArray(), solutionDetails);
        }
    }
}
