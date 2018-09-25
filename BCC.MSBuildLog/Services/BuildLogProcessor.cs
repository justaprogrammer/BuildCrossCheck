using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Interfaces;
using BCC.MSBuildLog.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace BCC.MSBuildLog.Services
{
    public class BuildLogProcessor : IBuildLogProcessor
    {
        private readonly IFileSystem _fileSystem;
        private readonly IBinaryLogProcessor _binaryLogProcessor;

        private ILogger<BuildLogProcessor> Logger { get; }

        public BuildLogProcessor(
            IFileSystem fileSystem,
            IBinaryLogProcessor binaryLogProcessor,
            ILogger<BuildLogProcessor> logger = null)
        {
            _fileSystem = fileSystem;
            _binaryLogProcessor = binaryLogProcessor;

            Logger = logger ?? new NullLogger<BuildLogProcessor>();
        }

        public void Proces(string inputFile, string outputFile, string cloneRoot, string configurationFile = null)
        {
            if (!_fileSystem.File.Exists(inputFile))
            {
                throw new InvalidOperationException($"Input file `{inputFile}` does not exist.");
            }

            if (_fileSystem.File.Exists(outputFile))
            {
                throw new InvalidOperationException($"Output file `{outputFile}` already exists.");
            }

            CheckRunConfiguration configuration = null;
            if (configurationFile != null)
            {
                if (!_fileSystem.File.Exists(configurationFile))
                {
                    throw new InvalidOperationException($"Configuration file `{configurationFile}` does not exist.");
                }

                var configurationString = _fileSystem.File.ReadAllText(configurationFile);
                if (string.IsNullOrWhiteSpace(configurationString))
                {
                    throw new InvalidOperationException($"Content of configuration file `{configurationFile}` is null or empty.");
                }

                configuration = JsonConvert.DeserializeObject<CheckRunConfiguration>(configurationString);
            }

            var dateTimeOffset = DateTimeOffset.Now;
            var logData = _binaryLogProcessor.ProcessLog(inputFile, cloneRoot, configuration);

            var hasAnyFailure = logData.Annotations.Any() &&
                                logData.Annotations.Any(annotation => annotation.CheckWarningLevel == CheckWarningLevel.Failure);

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(logData.ErrorCount.ToString());
            stringBuilder.Append(" ");
            stringBuilder.Append(logData.ErrorCount == 1 ? "Error": "Errors");
            stringBuilder.Append(" ");
            stringBuilder.Append(logData.WarningCount.ToString());
            stringBuilder.Append(" ");
            stringBuilder.Append(logData.WarningCount == 1 ? "Warning" : "Warnings");

            var contents = JsonConvert.SerializeObject(new CreateCheckRun
            {
                Annotations = logData.Annotations,
                Success = !hasAnyFailure,
                StartedAt = dateTimeOffset,
                CompletedAt = DateTimeOffset.Now,
                Summary = string.Empty,
                Name = configuration?.Name ?? "MSBuild Log",
                Title = stringBuilder.ToString(),
            });

            _fileSystem.File.WriteAllText(outputFile, contents);
        }
    }
}