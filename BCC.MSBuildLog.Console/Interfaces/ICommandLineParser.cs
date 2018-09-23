namespace BCC.MSBuildLog.Console.Interfaces
{
    public interface ICommandLineParser
    {
        ApplicationArguments Parse(string[] args);
    }
}