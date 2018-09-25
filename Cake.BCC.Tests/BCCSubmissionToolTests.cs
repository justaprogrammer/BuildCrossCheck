using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Tooling;
using NSubstitute;
using Xunit;

namespace Cake.BCC.Tests
{
    public class BCCSubmissionToolTests
    {
        [Fact]
        public void ShouldContrsuct()
        {
            var bccmsBuildLogTool = new BCCSubmissionTool(Substitute.For<IFileSystem>(), Substitute.For<ICakeEnvironment>(), Substitute.For<IProcessRunner>(), Substitute.For<IToolLocator>());
        }
    }
}