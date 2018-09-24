namespace BCC.MSBuildLog.Model
{
    public class CheckRunConfiguration   {
        public LogAnalyzerRule[] Rules { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }
}