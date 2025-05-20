using System.Net.Http.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using AuthService.Tests;
using Microsoft.AspNetCore.Hosting; // Ensure IWebHostBuilder is recognized
using AuthService.DTOs;

namespace AuthService.Tests.IntegrationTests
{
    public class AuthServiceRegistrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthServiceRegistrationTests(CustomWebApplicationFactory<Program> factory)
        {
            // Use the custom WebApplicationFactory to create an HttpClient
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RegisterAsync_ShouldCreateUser_WhenDataIsValid()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "newuser@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.IsSuccessStatusCode.Should().BeTrue();
            var responseContent = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
            responseContent.Should().NotBeNull();
            responseContent!.Message.Should().Be("User registered successfully.");
        }

        [Fact]
        public async Task RegisterAsync_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            var existingUserDto = new RegisterDto
            {
                Username = "existinguser",
                Email = "test@example.com",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Prepopulate the database with an existing user
            await _client.PostAsJsonAsync("/api/auth/register", existingUserDto);

            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "test@example.com", // Same email as the existing user
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
            var responseContent = await response.Content.ReadAsStringAsync();
            responseContent.Should().Contain("Email already exists.");
        }
    }
}