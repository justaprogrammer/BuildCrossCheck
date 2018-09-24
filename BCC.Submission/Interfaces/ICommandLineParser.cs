namespace BCC.Submission.Interfaces
{
    public interface ICommandLineParser
    {
        ApplicationArguments Parse(string[] args);
    }
}