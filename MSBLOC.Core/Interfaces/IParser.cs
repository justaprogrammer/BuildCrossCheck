using MSBLOC.Core.Model;

namespace MSBLOC.Core.Interfaces
{
    public interface IParser
    {
        ParsedBinaryLog Parse(string resourcePath, string cloneRoot = "");
    }
}