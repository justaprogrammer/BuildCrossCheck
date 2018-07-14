using Microsoft.Build.Framework;
using Octokit;
using Shouldly;

namespace MSBLOC.Core.Tests.Services
{
    internal static class ShoudlyExtensions
    {
        public static void ShouldBe(this BuildErrorEventArgs buildErrorEventArgs, BuildErrorEventArgs expected)
        {
            buildErrorEventArgs.Subcategory.ShouldBe(expected.Subcategory);
            buildErrorEventArgs.Code.ShouldBe(expected.Code);
            buildErrorEventArgs.File.ShouldBe(expected.File);
            buildErrorEventArgs.LineNumber.ShouldBe(expected.LineNumber);
            buildErrorEventArgs.ColumnNumber.ShouldBe(expected.ColumnNumber);
            buildErrorEventArgs.EndLineNumber.ShouldBe(expected.EndLineNumber);
            buildErrorEventArgs.EndColumnNumber.ShouldBe(expected.EndColumnNumber);
            buildErrorEventArgs.Message.ShouldBe(expected.Message);
            buildErrorEventArgs.HelpKeyword.ShouldBe(expected.HelpKeyword);
            buildErrorEventArgs.SenderName.ShouldBe(expected.SenderName);
            buildErrorEventArgs.ProjectFile.ShouldBe(expected.ProjectFile);
        }

        public static void ShouldBe(this BuildWarningEventArgs buildWarningEventArgs, BuildWarningEventArgs expected)
        {
            buildWarningEventArgs.Subcategory.ShouldBe(expected.Subcategory);
            buildWarningEventArgs.Code.ShouldBe(expected.Code);
            buildWarningEventArgs.File.ShouldBe(expected.File);
            buildWarningEventArgs.LineNumber.ShouldBe(expected.LineNumber);
            buildWarningEventArgs.ColumnNumber.ShouldBe(expected.ColumnNumber);
            buildWarningEventArgs.EndLineNumber.ShouldBe(expected.EndLineNumber);
            buildWarningEventArgs.EndColumnNumber.ShouldBe(expected.EndColumnNumber);
            buildWarningEventArgs.Message.ShouldBe(expected.Message);
            buildWarningEventArgs.HelpKeyword.ShouldBe(expected.HelpKeyword);
            buildWarningEventArgs.SenderName.ShouldBe(expected.SenderName);
            buildWarningEventArgs.ProjectFile.ShouldBe(expected.ProjectFile);
        }

        public static void ShouldBe(this NewCheckRun newCheckRun, string checkRunTitle, string checkRunSummary,
            NewCheckRunAnnotation[] expectedAnnotations, NewCheckRun expectedCheckRun)
        {
            newCheckRun.Name.ShouldBe(expectedCheckRun.Name);
            newCheckRun.HeadSha.ShouldBe(expectedCheckRun.HeadSha);
            newCheckRun.Output.Title.ShouldBe(checkRunTitle);
            newCheckRun.Output.Summary.ShouldBe(checkRunSummary);

            newCheckRun.Output.Annotations.Count.ShouldBe(expectedAnnotations.Length);

            for (var index = 0; index < newCheckRun.Output.Annotations.Count; index++)
            {
                var newCheckRunAnnotation = newCheckRun.Output.Annotations[index];
                var expectedAnnotation = expectedAnnotations[index];
            }
        }
    }
}