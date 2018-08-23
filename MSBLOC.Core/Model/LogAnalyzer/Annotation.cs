using MSBLOC.Core.Model.LogAnalyzer;

namespace MSBLOC.Core.Model
{
    public class Annotation
    {
        public string Filename { get; }
        public CheckWarningLevel CheckWarningLevel { get; }
        public string Title { get; }
        public string Message { get; }
        public int LineNumber { get; }
        public int EndLine { get; }
        public string BlobHref { get; }

        public Annotation(string filename, CheckWarningLevel checkWarningLevel, string title, string message,
            int lineNumber, int endLine, string blobHref)
        {
            Filename = filename;
            BlobHref = blobHref;
            CheckWarningLevel = checkWarningLevel;
            Title = title;
            Message = message;
            LineNumber = lineNumber;
            EndLine = endLine;
        }
    }
}