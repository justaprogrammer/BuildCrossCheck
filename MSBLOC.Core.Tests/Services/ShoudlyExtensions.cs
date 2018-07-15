using FluentAssertions;
using Microsoft.Build.Framework;
using Octokit;

namespace MSBLOC.Core.Tests.Services
{
    internal static class ShoudlyExtensions
    {
        public static void ShouldBe(this NewCheckRun newCheckRun, string checkRunTitle, string checkRunSummary,
            NewCheckRunAnnotation[] expectedAnnotations, NewCheckRun expectedCheckRun)
        {
            newCheckRun.Name.Should().Be(expectedCheckRun.Name);
            newCheckRun.HeadSha.Should().Be(expectedCheckRun.HeadSha);
            newCheckRun.Output.Title.Should().Be(checkRunTitle);
            newCheckRun.Output.Summary.Should().Be(checkRunSummary);

            newCheckRun.Output.Annotations.Count.Should().Be(expectedAnnotations.Length);

            for (var index = 0; index < newCheckRun.Output.Annotations.Count; index++)
            {
                var newCheckRunAnnotation = newCheckRun.Output.Annotations[index];
                var expectedAnnotation = expectedAnnotations[index];
            }
        }
    }
}