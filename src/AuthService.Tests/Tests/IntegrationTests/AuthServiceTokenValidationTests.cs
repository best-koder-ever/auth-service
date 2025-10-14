using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace AuthService.Tests.IntegrationTests
{
    public class AuthServiceTokenValidationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthServiceTokenValidationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PublicKeyEndpoint_Returns410Gone_WithKeycloakJwks()
        {
            using var response = await _client.GetAsync("/api/publickey");

            response.StatusCode.Should().Be(HttpStatusCode.Gone);

            var raw = await response.Content.ReadAsStringAsync();
            raw.Should().NotBeNullOrWhiteSpace();

            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            root.GetProperty("message").GetString().Should().Contain("Keycloak");
            var jwks = root.GetProperty("jwks").GetString();
            jwks.Should().NotBeNullOrWhiteSpace();
            jwks.Should().EndWith("/protocol/openid-connect/certs");
        }
    }
}