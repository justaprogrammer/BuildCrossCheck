using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using MSBLOC.Core.Services;

namespace MSBLOC.Core.Model
{
    public class BuildDetails
    {
        public List<Annotation> Annotations { get; }

        public SolutionDetails SolutionDetails { get; }

        public BuildDetails(SolutionDetails solutionDetails, IEnumerable<Annotation> annotations = null)
        {
            SolutionDetails = solutionDetails;
            Annotations = annotations?.ToList() ?? new List<Annotation>();
        }

        public void AddAnnotation(string filename, int lineNumber, int endLine, AnnotationWarningLevel checkWarningLevel, string message, string title)
        {
            var annotation = new Annotation(filename, checkWarningLevel, title, message, lineNumber, endLine);
            Annotations.Add(annotation);
        }
    }

    public class Annotation
    {
        public string Filename { get; }
        public AnnotationWarningLevel AnnotationWarningLevel { get; }
        public string Title { get; }
        public string Message { get; }
        public int LineNumber { get; }
        public int EndLine { get; }

        public Annotation(string filename, AnnotationWarningLevel annotationWarningLevel, string title, string message,
            int lineNumber, int endLine)
        {
            Filename = filename;
            AnnotationWarningLevel = annotationWarningLevel;
            Title = title;
            Message = message;
            LineNumber = lineNumber;
            EndLine = endLine;
        }
    }

    public enum AnnotationWarningLevel
    {
        Notice,
        Warning,
        Failure,
    }
}