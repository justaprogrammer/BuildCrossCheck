using MSBLOC.Core.Model;

namespace MSBLOC.Core.Interfaces
{
    public interface IParser
    {
        BuildDetails Parse(string filePath, string cloneRoot = "");
    }
}