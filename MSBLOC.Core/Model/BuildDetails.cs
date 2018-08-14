using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using MSBLOC.Core.Services;

namespace MSBLOC.Core.Model
{
    public class BuildDetails
    {
        public IList<Annotation> Annotations { get; }

        public SolutionDetails SolutionDetails { get; }

        public BuildDetails(SolutionDetails solutionDetails, IEnumerable<Annotation> annotations = null)
        {
            SolutionDetails = solutionDetails;
            Annotations = annotations?.ToList() ?? new List<Annotation>();
        }

        public void AddAnnotation(string filename, int lineNumber, int endLine, CheckWarningLevel checkWarningLevel, string message, string title)
        {
            var annotation = new Annotation(filename, checkWarningLevel, title, message, lineNumber, endLine);
            Annotations.Add(annotation);
        }
    }
}