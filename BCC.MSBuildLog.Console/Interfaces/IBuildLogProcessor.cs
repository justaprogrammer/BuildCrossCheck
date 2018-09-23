namespace BCC.MSBuildLog.Console.Interfaces
{
    public interface IBuildLogProcessor
    {
        void Proces(string inputFile, string outputFile, string cloneRoot);
    }
}