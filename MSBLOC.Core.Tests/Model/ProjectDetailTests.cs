using System;
using Microsoft.Extensions.Logging;
using MSBLOC.Core.Model;
using MSBLOC.Core.Tests.Util;
using Xunit;
using Xunit.Abstractions;

namespace MSBLOC.Core.Tests.Model
{
    public class ProjectDetailTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<ProjectDetailTests> _logger;

        public ProjectDetailTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<ProjectDetailTests>(testOutputHelper);
        }

        [Fact]
        public void ShouldThrowWhenAddingFileTwice()
        {
            var projectDetails = new ProjectDetails(@"c:\Project\", @"c:\Project\Project1.csproj");
            projectDetails.AddItems(@"c:\Project\File1.cs");
            Assert.Throws<ArgumentException>(() => projectDetails.AddItems(@"c:\Project\File1.cs"));
        }
    }
}