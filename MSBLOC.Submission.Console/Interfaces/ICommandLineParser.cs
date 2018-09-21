namespace MSBLOC.Submission.Console.Interfaces
{
    public interface ICommandLineParser
    {
        ApplicationArguments Parse(string[] args);
    }
}