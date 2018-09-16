namespace MSBLOC.MSBuildLog.Console.Interfaces
{
    public interface ICommandLineParser
    {
        ApplicationArguments Parse(string[] args);
    }
}