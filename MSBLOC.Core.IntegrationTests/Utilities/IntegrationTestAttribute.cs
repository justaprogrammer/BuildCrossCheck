using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace MSBLOC.Core.IntegrationTests.Utilities
{
    public class IntegrationTestDiscoverer : IXunitTestCaseDiscoverer
    {
        readonly IMessageSink _diagnosticMessageSink;

        public IntegrationTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            this._diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            return Helper.HasCredentials
                ? new[] { new XunitTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod) }
                : Enumerable.Empty<IXunitTestCase>();
        }
    }

    [XunitTestCaseDiscoverer("MSBLOC.Core.IntegrationTests.Utilities.IntegrationTestDiscoverer", "MSBLOC.Core.IntegrationTests")]
    public class IntegrationTestAttribute : FactAttribute
    {
    }
}
