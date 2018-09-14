namespace MSBLOC.MSBuildLog.Console
{
    public interface ICommandLineParser
    {
        ApplicationArguments Parse(string[] args);
    }
}