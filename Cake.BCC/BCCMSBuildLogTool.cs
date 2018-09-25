using System;
using System.Collections.Generic;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Cake.Core.Tooling;

namespace Cake.BCC
{
    public class BCCMSBuildLogTool : Tool<BCCMSBuildLogToolSettings>
    {
        private readonly ICakeEnvironment _environment;

        public BCCMSBuildLogTool(IFileSystem fileSystem, ICakeEnvironment environment, IProcessRunner runner, IToolLocator tools)
            : base(fileSystem, environment, runner, tools)
        {
            _environment = environment;
        }

        public void BCCMSBuildLog(BCCMSBuildLogToolSettings settings)
        {
            var arguments = new ProcessArgumentBuilder();

//            var filePath = FilePath.FromString($"bin\\{settings.Configuration}\\{settings.Framework}\\{project.Name}.dll");
//            var fullPath = project.Path.GetDirectory().CombineWithFilePath(filePath).MakeAbsolute(_environment);
//            arguments.Append($"\"{fullPath}\" --target \"dotnet\" --targetargs \"test -c {settings.Configuration} {project.Path.FullPath} --no-build\" --format opencover --output \"{settings.Output}\"");

            Run(settings, arguments);
        }

        protected override string GetToolName()
        {
            return "BCCMSBuildLog";
        }

        protected override IEnumerable<string> GetToolExecutableNames()
        {
            return new[] { "coverlet", "coverlet.exe" };
        }
    }

    public class BCCMSBuildLogToolSettings : ToolSettings
    {
    }

    public static class CoverletAliases
    {
        [CakeMethodAlias]
        public static void BCCMSBuildLog(this ICakeContext context, BCCMSBuildLogToolSettings settings = null)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            new BCCMSBuildLogTool(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools).BCCMSBuildLog(settings);
        }
    }
}
