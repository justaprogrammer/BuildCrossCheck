namespace MSBLOC.Core.Model.LogAnalyzer
{
    public class Annotation
    {
        public string Filename { get; }
        public CheckWarningLevel CheckWarningLevel { get; }
        public string Title { get; }
        public string Message { get; }
        public int LineNumber { get; }
        public int EndLine { get; }

        public Annotation(string filename, CheckWarningLevel checkWarningLevel, string title, string message,
            int lineNumber, int endLine)
        {
            Filename = filename;
            CheckWarningLevel = checkWarningLevel;
            Title = title;
            Message = message;
            LineNumber = lineNumber;
            EndLine = endLine;
        }
    }
}