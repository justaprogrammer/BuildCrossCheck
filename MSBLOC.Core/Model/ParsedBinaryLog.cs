using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace MSBLOC.Core.Model
{
    public class ParsedBinaryLog
    {
        public BuildWarningEventArgs[] Warnings { get; }

        public BuildErrorEventArgs[] Errors { get; }

        public ParsedBinaryLog(BuildWarningEventArgs[] warnings, BuildErrorEventArgs[] errors)
        {
            Warnings = warnings;
            Errors = errors;
        }
    }
}