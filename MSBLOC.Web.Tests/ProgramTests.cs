using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace MSBLOC.Web.Tests
{
    public class ProgramTests
    {
        [Fact]
        public void ConfigureAppConfigurationAzureTest()
        {
            var context = Substitute.For<WebHostBuilderContext>();
            
            var configRoot = Substitute.For<IConfigurationRoot>();
            configRoot["Azure:KeyVault"].Returns("NameOfVault");

            var config = Substitute.For<IConfigurationBuilder>();
            config.Add(Arg.Do<EnvironmentVariablesConfigurationSource>(arg =>
            {
                arg.Prefix.Should().Be("MSBLOC_");
            }));
            config.Build().Returns(configRoot);

            Assert.Throws<HttpRequestException>(() => Program.MSBLOCConfigureAppConfiguration(context, config));

            config.Received().Add(Arg.Any<EnvironmentVariablesConfigurationSource>());
            config.Received().Build();
        }

        [Fact]
        public void ConfigureAppConfigurationNotAzureTest()
        {
            var context = Substitute.For<WebHostBuilderContext>();

            var configRoot = Substitute.For<IConfigurationRoot>();
            configRoot["Azure:KeyVault"].Returns((string) null);

            var config = Substitute.For<IConfigurationBuilder>();
            config.Add(Arg.Do<EnvironmentVariablesConfigurationSource>(arg =>
            {
                arg.Prefix.Should().Be("MSBLOC_");
            }));
            config.Build().Returns(configRoot);

            Program.MSBLOCConfigureAppConfiguration(context, config);

            config.Received().Add(Arg.Any<EnvironmentVariablesConfigurationSource>());
            config.Received().Build();
        }
    }
}
