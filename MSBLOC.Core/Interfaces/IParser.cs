using MSBLOC.Core.Models;

namespace MSBLOC.Core.Interfaces
{
    public interface IParser
    {
        StubAnnotation[] Parse(string resourcePath);
    }
}