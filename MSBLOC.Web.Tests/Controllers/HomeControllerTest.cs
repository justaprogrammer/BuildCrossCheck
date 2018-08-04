using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MSBLOC.Web;
using MSBLOC.Web.Controllers;
using Xunit;

namespace MSBLOC.Web.Tests.Controllers
{
    public class HomeControllerTest
    {
        [Fact]
        public async Task Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = (await controller.Index()) as ViewResult;

            // Assert
            Assert.NotNull(result);
        }
    }
}
