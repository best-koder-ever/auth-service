using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Tests.EndToEndTests
{
    public class AuthServiceEndToEndTests : IClassFixture<WebApplicationFactory<AuthService.Startup>>
    {
        private readonly HttpClient _client;

        public AuthServiceEndToEndTests(WebApplicationFactory<AuthService.Startup> factory)
        {
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:8081")
            });
        }

        [Fact]
        public async Task RegisterAndLogin_EndpointReturnsOk()
        {
            // Arrange
            var registerContent = new StringContent("{\"username\":\"testuser\",\"email\":\"testuser@example.com\",\"password\":\"Test@1234\",\"phoneNumber\":\"1234567890\"}", System.Text.Encoding.UTF8, "application/json");
            var loginContent = new StringContent("{\"email\":\"testuser@example.com\",\"password\":\"Test@1234\"}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var registerResponse = await _client.PostAsync("/api/auth/register", registerContent);
            registerResponse.EnsureSuccessStatusCode();

            var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);
            loginResponse.EnsureSuccessStatusCode();

            var responseString = await loginResponse.Content.ReadAsStringAsync();
            Assert.Contains("token", responseString);
        }
    }
}