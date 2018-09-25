using System;
using System.Collections.Generic;
using Cake.Common.Tools.DotNetCore;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Cake.Core.Tooling;
using Cake.Frosting;

namespace Cake.BCC
{
    public class BCCSubmissionTool : Tool<BCCSubmissionToolSettings>
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICakeEnvironment _environment;

        public BCCSubmissionTool(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner runner, IToolLocator tools)
            : base(fileSystem, environment, runner, tools)
        {
            _fileSystem = fileSystem;
            _environment = environment;
        }

        public void BCCMSBuildLog(ICakeContext context, string version, BCCSubmissionToolSettings settings)
        {
            var programDllPath = _fileSystem.GetDirectory(_environment.GetEnvironmentVariable("USERPROFILE"))
                .Path.CombineWithFilePath(
                    $".nuget\\packages\\bcc-msbuildlog\\{version}\\tools\\netcoreapp2.1\\BCC.MSBuildLog.dll"
                )
                .MakeAbsolute(_environment).FullPath;

            context.DotNetCoreExecute(programDllPath);

            // var arguments = new ProcessArgumentBuilder();
//            context.Directory(context.EnvironmentVariable("USERPROFILE"))
//                .Path.CombineWithFilePath(".nuget\\packages\\msbuild.structuredlogger\\2.0.11\\lib\\netstandard2.0\\StructuredLogger.dll")
//                .MakeAbsolute(context.Environment)
//                .FullPath;

            //            var filePath = FilePath.FromString($"bin\\{settings.Configuration}\\{settings.Framework}\\{project.Name}.dll");
            //            var fullPath = project.Path.GetDirectory().CombineWithFilePath(filePath).MakeAbsolute(_environment);
            //            arguments.Append($"\"{fullPath}\" --target \"dotnet\" --targetargs \"test -c {settings.Configuration} {project.Path.FullPath} --no-build\" --format opencover --output \"{settings.Output}\"");

//            Run(settings, arguments);
        }

        protected override string GetToolName()
        {
            return "BCCSubmission";
        }

        protected override IEnumerable<string> GetToolExecutableNames()
        {
            return new[] { "coverlet", "coverlet.exe" };
        }
    }

    public class BCCSubmissionToolSettings : ToolSettings
    {
        public string InputPath { get; set; }
        public string Token { get; set; }
        public string HeadSha { get; set; }
    }

    public static class BCCSubmissionToolAliases
    {
        [CakeMethodAlias]
        public static void BCCSubmission(this ICakeContext context, string version,
            BCCMSBuildLogToolSettings settings = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            new BCCMSBuildLogTool(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools)
                .BCCMSBuildLog(context, version, settings);
        }
    }
}
