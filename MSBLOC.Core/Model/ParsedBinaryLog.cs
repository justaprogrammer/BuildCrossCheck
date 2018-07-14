using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace MSBLOC.Core.Model
{
    public class ParsedBinaryLog
    {
        public IList<BuildWarningEventArgs> Warnings { get; }

        public IList<BuildErrorEventArgs> Errors { get; }

        public ParsedBinaryLog(IList<BuildWarningEventArgs> warnings, IList<BuildErrorEventArgs> errors)
        {
            Warnings = warnings;
            Errors = errors;
        }
    }
}