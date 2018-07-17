using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace MSBLOC.Core.Model
{
    public class ParsedBinaryLog
    {
        public BuildWarningEventArgs[] Warnings { get; }

        public BuildErrorEventArgs[] Errors { get; }

        public ParsedBinaryLog(BuildWarningEventArgs[] warnings = null, BuildErrorEventArgs[] errors = null)
        {
            Warnings = warnings ?? new BuildWarningEventArgs[0];
            Errors = errors ?? new BuildErrorEventArgs[0];
        }
    }
}