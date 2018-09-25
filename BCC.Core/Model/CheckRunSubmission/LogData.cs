namespace BCC.Core.Model.CheckRunSubmission
{
    public class LogData
    {
        public int WarningCount { get; set; }
        public int ErrorCount { get; set; }
        public Annotation[] Annotations { get; set; }
    }
}