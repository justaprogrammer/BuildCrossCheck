using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace MSBLOC.Core.Model
{
    public class ParsedBinaryLog
    {
        public BuildWarningEventArgs[] Warnings { get; }

        public BuildErrorEventArgs[] Errors { get; }

        public Dictionary<string, Dictionary<string, string>> ProjectFileLookup { get; }

        public ParsedBinaryLog(BuildWarningEventArgs[] warnings, BuildErrorEventArgs[] errors,
            Dictionary<string, Dictionary<string, string>> projectFileLookup)
        {
            Warnings = warnings;
            Errors = errors;
            ProjectFileLookup = projectFileLookup;
        }
    }
}