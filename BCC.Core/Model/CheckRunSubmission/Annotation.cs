namespace BCC.Core.Model.CheckRunSubmission
{
    public class Annotation
    {
        public string Filename { get; set;  }
        public CheckWarningLevel CheckWarningLevel { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public int LineNumber { get; set; }
        public int EndLine { get; set; }

        public Annotation()
        {
        }

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