namespace BCC.MSBuildLog.Interfaces
{
    public interface IBuildLogProcessor
    {
        void Proces(string inputFile, string outputFile, string cloneRoot);
    }
}