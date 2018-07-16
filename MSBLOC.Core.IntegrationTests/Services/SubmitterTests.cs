using Microsoft.Extensions.Logging;
using MSBLOC.Core.IntegrationTests.Utilities;
using MSBLOC.Core.Tests.Util;
using Xunit.Abstractions;

namespace MSBLOC.Core.IntegrationTests.Services
{
    public class SubmitterIntegrationTests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly ILogger<SubmitterIntegrationTests> _logger;

        public SubmitterIntegrationTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _logger = TestLogger.Create<SubmitterIntegrationTests>(testOutputHelper);
        }

        [IntegrationTest]
        public void ShouldSubmit()
        {

        }
    }
}