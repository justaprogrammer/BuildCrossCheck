using System;
using System.IO.Abstractions;
using System.Linq;
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
            var annotations = _binaryLogProcessor.CreateAnnotations(inputFile, cloneRoot, configuration).ToArray();

            var hasAnyFailure = annotations.Any() &&
                annotations.Any(annotation => annotation.CheckWarningLevel == CheckWarningLevel.Failure);

            var contents = JsonConvert.SerializeObject(new CreateCheckRun
            {
                Annotations = annotations,
                Success = !hasAnyFailure,
                StartedAt = dateTimeOffset,
                CompletedAt = DateTimeOffset.Now,
                Summary = string.Empty,
                Name = configuration?.Name ?? "MSBuild Analyzer",
                Title = configuration?.Title ?? "MSBuild Log Messages",
            });

            _fileSystem.File.WriteAllText(outputFile, contents);
        }
    }
}