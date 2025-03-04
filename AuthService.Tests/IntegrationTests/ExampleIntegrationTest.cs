using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Tests.IntegrationTests
{
    public class ExampleIntegrationTest : IClassFixture<WebApplicationFactory<AuthService.Startup>>
    {
        private readonly HttpClient _client;

        public ExampleIntegrationTest(WebApplicationFactory<AuthService.Startup> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Get_EndpointReturnsOk()
        {
            // Act
            var response = await _client.GetAsync("/api/auth");

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("expected content", responseString);
        }
    }
}