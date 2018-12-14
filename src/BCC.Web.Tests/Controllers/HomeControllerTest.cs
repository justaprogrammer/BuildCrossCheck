using BCC.Web.Controllers;
using BCC.Web.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace BCC.Web.Tests.Controllers
{
    public class HomeControllerTest
    {
        [Fact]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController(Substitute.For<ITelemetryService>());

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
        }
    }
}
