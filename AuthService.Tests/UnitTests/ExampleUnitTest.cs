using Xunit;
using Moq;
using AuthService.Controllers;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Tests.UnitTests
{
    public class ExampleUnitTest
    {
        [Fact]
        public void Get_ReturnsOkResult()
        {
            // Arrange
            var mockService = new Mock<IAuthService>();
            var controller = new AuthController(mockService.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}