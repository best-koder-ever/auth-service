using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AuthService.Tests;
using Microsoft.AspNetCore.Hosting; // Ensure IWebHostBuilder is recognized

namespace AuthService.Tests.IntegrationTests
{
    public class AuthServiceTokenValidationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthServiceTokenValidationTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        private string GenerateValidToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKey12345678901234567890")); // 256-bit key
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "TestIssuer",
                audience: "TestAudience",
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnOk_WhenTokenIsValid()
        {
            // Arrange
            var validToken = GenerateValidToken();
            var content = new StringContent(validToken, Encoding.UTF8, "text/plain");

            // Act
            var response = await _client.PostAsync("/api/auth/validate", content);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Token is valid.");
        }

        [Fact]
        public async Task ValidateToken_ShouldReturnUnauthorized_WhenTokenIsInvalid()
        {
            // Arrange
            var invalidToken = "invalid-token";
            var content = new StringContent(invalidToken, Encoding.UTF8, "text/plain");

            // Act
            var response = await _client.PostAsync("/api/auth/validate", content);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Invalid token.");
        }
    }
}