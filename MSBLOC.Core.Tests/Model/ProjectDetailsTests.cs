using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model;
using MSBLOC.Core.Model.Builds;
using MSBLOC.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Model
{
    public class ProjectDetailsTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<ProjectDetailsTests> _logger;

        public ProjectDetailsTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<ProjectDetailsTests>(testOutputHelper);
        }

        [Fact]
        public void ShouldThrowWhenAddingFileTwice()
        {
            var projectDetails = new ProjectDetails(@"c:\Project\", @"c:\Project\Project1.csproj");
            projectDetails.AddItems(@"c:\Project\File1.cs");

            var argumentException = Assert.Throws<ProjectDetailsException>(() => projectDetails.AddItems(@"c:\Project\File1.cs"));
            argumentException.Message.Should().Be(@"Item ""c:\Project\File1.cs"" already exists");
        }

        [Fact]
        public void ShouldConstructWhenProjectPathIsChildOfClone()
        {
            new ProjectDetails(@"c:\Project\", @"c:\Project\Project1.csproj");
        }

        [Fact]
        public void ShouldThrowOnConstructWhenProjectPathIsNotChildOfClone()
        {
            var argumentException = Assert.Throws<ProjectDetailsException>(() => new ProjectDetails(@"c:\Projects\", @"c:\Project\Project1.csproj"));
            argumentException.Message.Should().Be(@"Project file path ""c:\Project\Project1.csproj"" is not a subpath of ""c:\Projects\""");
        }
    }
}