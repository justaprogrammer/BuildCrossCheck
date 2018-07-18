using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace MSBLOC.Core.Model
{
    public class ParsedBinaryLog
    {
        public BuildWarningEventArgs[] Warnings { get; }

        public BuildErrorEventArgs[] Errors { get; }

        public Dictionary<string, Dictionary<string, string>> ProjectFileLookup { get; }

        public ParsedBinaryLog(BuildWarningEventArgs[] warnings = null, BuildErrorEventArgs[] errors = null,
            Dictionary<string, Dictionary<string, string>> projectFileLookup = null)
        {
            Warnings = warnings ?? new BuildWarningEventArgs[0];
            Errors = errors ?? new BuildErrorEventArgs[0];
            ProjectFileLookup = projectFileLookup ?? new Dictionary<string, Dictionary<string, string>>();
        }
    }
}