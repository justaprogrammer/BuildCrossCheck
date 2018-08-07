using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MSBLOC.Web.Models;
using MSBLOC.Web.Services;
using NSubstitute;
using Xunit;

namespace MSBLOC.Web.Tests.Services
{
    public class OptionsPrivateKeySourceTests
    {
        [Fact]
        public void GetPrivateKeyReaderTest()
        {
            var options = new GitHubAppOptions
            {
                PrivateKey = "123456"
            };
            var optionsAccessor = Substitute.For<IOptions<GitHubAppOptions>>();
            optionsAccessor.Value.Returns(options);

            var keySource = new GitHubAppOptionsPrivateKeySource(optionsAccessor);

            var reader = keySource.GetPrivateKeyReader();

            var privateKey = reader.ReadToEnd();

            privateKey.Should().Be($"-----BEGIN RSA PRIVATE KEY-----\r\n{options.PrivateKey}\r\n-----END RSA PRIVATE KEY-----\r\n");
        }
    }
}
