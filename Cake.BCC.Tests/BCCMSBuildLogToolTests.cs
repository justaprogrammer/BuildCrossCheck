using System;
using Cake.Core;
using Cake.Core.IO;
using Cake.Core.Tooling;
using NSubstitute;
using Xunit;

namespace Cake.BCC.Tests
{
    public class BCCMSBuildLogToolTests
    {
        [Fact]
        public void Test1()
        {
            var bccmsBuildLogTool = new BCCMSBuildLogTool(Substitute.For<IFileSystem>(), Substitute.For<ICakeEnvironment>(), Substitute.For<IProcessRunner>(), Substitute.For<IToolLocator>());
        }
    }
}
