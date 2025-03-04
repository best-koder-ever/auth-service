using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Tests.IntegrationTests.AuthControllerTests
{
    public class LoginTests : IClassFixture<WebApplicationFactory<AuthService.Startup>>
    {
        private readonly HttpClient _client;

        public LoginTests(WebApplicationFactory<AuthService.Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_EndpointReturnsOk()
        {
            // Arrange
            var content = new StringContent("{\"email\":\"testuser@example.com\",\"password\":\"Test@1234\"}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/auth/login", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", responseString);
        }
    }
}