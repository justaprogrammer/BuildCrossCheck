using Octokit;

namespace MSBLOC.Core.Interfaces
{
    public interface IParser
    {
        CheckRunAnnotation[] Parse(string resourcePath);
    }
}