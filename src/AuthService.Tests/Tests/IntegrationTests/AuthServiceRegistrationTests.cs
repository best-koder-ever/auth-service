using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Tests.IntegrationTests
{
    public class AuthServiceRegistrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthServiceRegistrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterEndpoint_ReturnsKeycloakMigrationDocumentationLink()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            using var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Gone);

            var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var documentation = json.RootElement.GetProperty("documentation").GetString();
            documentation.Should().Be("https://docs.yourdatingapp.com/keycloak-migration");
        }
    }
}