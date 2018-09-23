using System;
using System.IO.Abstractions;
using System.Linq;
using BCC.Core.Model.CheckRunSubmission;
using BCC.MSBuildLog.Interfaces;
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

        public void Proces(string inputFile, string outputFile, string cloneRoot)
        {
            if (!_fileSystem.File.Exists(inputFile))
            {
                throw new InvalidOperationException($"Input file `{inputFile}` does not exist");
            }

            if (_fileSystem.File.Exists(outputFile))
            {
                throw new InvalidOperationException($"Output file `{outputFile}` already exists");
            }

            var dateTimeOffset = DateTimeOffset.Now;
            var annotations = _binaryLogProcessor.ProcessLog(inputFile, cloneRoot).ToArray();

            var hasAnyFailure = annotations.Any() &&
                annotations.Any(annotation => annotation.CheckWarningLevel == CheckWarningLevel.Failure);

            var contents = JsonConvert.SerializeObject(new CreateCheckRun()
            {
                Annotations = annotations,
                Success = !hasAnyFailure,
                StartedAt = dateTimeOffset,
                CompletedAt = DateTimeOffset.Now,
                Summary = string.Empty,
                Name = "MSBuildLog Analyzer",
                Title = "MSBuildLog Analysis",
            });

            _fileSystem.File.WriteAllText(outputFile, contents);
        }
    }
}