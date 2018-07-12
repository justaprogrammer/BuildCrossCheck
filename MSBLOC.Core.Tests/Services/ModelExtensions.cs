using Octokit;
using Shouldly;

namespace MSBLOC.Core.Tests.Services
{
    internal static class ShoudlyExtensions
    {
        public static void ShouldBe(this CheckRunAnnotation stubAnnotation, CheckRunAnnotation expected)
        {
            stubAnnotation.Filename.ShouldBe(expected.Filename);
            stubAnnotation.BlobHref.ShouldBe(expected.BlobHref);
            stubAnnotation.StartLine.ShouldBe(expected.StartLine);
            stubAnnotation.EndLine.ShouldBe(expected.EndLine);
            stubAnnotation.WarningLevel.ShouldBe(expected.WarningLevel);
            stubAnnotation.Message.ShouldBe(expected.Message);
            stubAnnotation.Title.ShouldBe(expected.Title);
            stubAnnotation.RawDetails.ShouldBe(expected.RawDetails);
        }
    }
}