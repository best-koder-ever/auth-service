using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AuthService.Tests.IntegrationTests.AuthControllerTests
{
    public class RegisterTests : IClassFixture<WebApplicationFactory<AuthService.Startup>>
    {
        private readonly HttpClient _client;

        public RegisterTests(WebApplicationFactory<AuthService.Startup> factory)
        {
            var webAppFactory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            });

            _client = webAppFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:8081")
            });
        }

        [Fact]
        public async Task Register_EndpointReturnsOk()
        {
            // Arrange
            var content = new StringContent("{\"username\":\"testuser\",\"email\":\"testuser@example.com\",\"password\":\"Test@1234\",\"phoneNumber\":\"1234567890\"}", System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/Auth/register", content);

            // Assert
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("token", responseString);
        }
    }
}