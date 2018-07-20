using System.Collections.Generic;
using Microsoft.Build.Framework;
using MSBLOC.Core.Services;

namespace MSBLOC.Core.Model
{
    public class ParsedBinaryLog
    {
        public BuildWarningEventArgs[] Warnings { get; }

        public BuildErrorEventArgs[] Errors { get; }

        public SolutionDetails SolutionDetails { get; }

        public ParsedBinaryLog(BuildWarningEventArgs[] warnings, BuildErrorEventArgs[] errors,
            SolutionDetails solutionDetails)
        {
            Warnings = warnings;
            Errors = errors;
            SolutionDetails = solutionDetails;
        }
    }
}