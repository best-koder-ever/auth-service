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
    public class AuthServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthServiceIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        public static IEnumerable<object[]> LegacyAuthEndpoints() =>
            new List<object[]>
            {
                new object[] { "POST", "/api/auth/register", "POST /api/auth/register" },
                new object[] { "POST", "/api/auth/login", "POST /api/auth/login" },
                new object[] { "POST", "/api/auth/login/facebook", "POST /api/auth/login/facebook" },
                new object[] { "POST", "/api/auth/login/google", "POST /api/auth/login/google" },
                new object[] { "POST", "/api/auth/login/phone", "POST /api/auth/login/phone" },
                new object[] { "POST", "/api/auth/refresh", "POST /api/auth/refresh" },
                new object[] { "POST", "/api/auth/logout", "POST /api/auth/logout" },
                new object[] { "POST", "/api/auth/forgot-password", "POST /api/auth/forgot-password" },
                new object[] { "POST", "/api/auth/verify-email", "POST /api/auth/verify-email" },
                new object[] { "POST", "/api/auth/validate", "POST /api/auth/validate" }
            };

        [Theory]
        [MemberData(nameof(LegacyAuthEndpoints))]
        public async Task LegacyAuthEndpoints_Return410Gone_WithMigrationMessage(string method, string path, string expectedEndpoint)
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), path)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            using var response = await _client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.Gone);

            var raw = await response.Content.ReadAsStringAsync();
            raw.Should().NotBeNullOrWhiteSpace();

            using var document = JsonDocument.Parse(raw);
            var root = document.RootElement;
            root.GetProperty("message").GetString().Should().Contain("Keycloak");
            root.GetProperty("endpoint").GetString().Should().Be(expectedEndpoint);
            root.GetProperty("documentation").GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task HealthEndpoint_RemainsAvailable()
        {
            using var response = await _client.GetAsync("/health");
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var parsed = JsonSerializer.Deserialize<string>(body);
            parsed.Should().Be("Healthy");
        }
    }
}