using System;
using JetBrains.Annotations;

namespace MSBLOC.Core.Model.Builds
{
    public class BuildMessage
    {
        public BuildMessage(
            BuildMessageLevel messageLevel,
            [NotNull] string projectFile, [NotNull] string file, int lineNumber,
            int endLineNumber, [NotNull] string message, [NotNull] string code)
        {
            MessageLevel = messageLevel;
            ProjectFile = projectFile ?? throw new ArgumentNullException(nameof(projectFile));
            File = file ?? throw new ArgumentNullException(nameof(file));
            LineNumber = lineNumber;
            EndLineNumber = endLineNumber == 0 ? lineNumber : endLineNumber;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Code = code ?? throw new ArgumentNullException(nameof(code));
        }

        public string ProjectFile { get; }
        public string File { get; }
        public int LineNumber { get; }
        public int EndLineNumber { get; }
        public string Message { get; }
        public string Code { get; }
        public BuildMessageLevel MessageLevel { get; }
    }
}