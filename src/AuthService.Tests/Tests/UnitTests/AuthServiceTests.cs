using System;
using System.Collections.Generic;
using AuthService.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AuthService.Tests.UnitTests
{
    public class AuthControllerTests
    {
        [Theory]
        [InlineData("register", "POST /api/auth/register")]
        [InlineData("login", "POST /api/auth/login")]
        [InlineData("login/facebook", "POST /api/auth/login/facebook")]
        [InlineData("login/google", "POST /api/auth/login/google")]
        [InlineData("login/phone", "POST /api/auth/login/phone")]
        [InlineData("refresh", "POST /api/auth/refresh")]
        [InlineData("logout", "POST /api/auth/logout")]
        [InlineData("forgot-password", "POST /api/auth/forgot-password")]
        [InlineData("verify-email", "POST /api/auth/verify-email")]
        [InlineData("validate", "POST /api/auth/validate")]
        public void LegacyEndpoints_Return410Gone_WithHelpfulPayload(string routeSuffix, string expectedEndpoint)
        {
            var controller = new AuthController(new NullLogger<AuthController>());

            IActionResult actionResult = routeSuffix switch
            {
                "register" => controller.Register(),
                "login" => controller.Login(),
                "login/facebook" => controller.LoginWithFacebook(),
                "login/google" => controller.LoginWithGoogle(),
                "login/phone" => controller.LoginWithPhoneNumber(),
                "refresh" => controller.Refresh(),
                "logout" => controller.Logout(),
                "forgot-password" => controller.ForgotPassword(),
                "verify-email" => controller.VerifyEmail(),
                "validate" => controller.ValidateToken(),
                _ => throw new ArgumentOutOfRangeException(nameof(routeSuffix))
            };

            var result = actionResult.Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(StatusCodes.Status410Gone);

            var payload = result.Value!;
            var message = payload.GetType().GetProperty("message")?.GetValue(payload) as string;
            var endpoint = payload.GetType().GetProperty("endpoint")?.GetValue(payload) as string;
            var documentation = payload.GetType().GetProperty("documentation")?.GetValue(payload) as string;

            message.Should().NotBeNullOrWhiteSpace().And.Contain("Keycloak");
            endpoint.Should().Be(expectedEndpoint);
            documentation.Should().NotBeNullOrWhiteSpace();
        }
    }

    public class PublicKeyControllerTests
    {
        [Fact]
        public void Get_Returns410Gone_AndPointsToKeycloakJwks()
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Keycloak:Authority"] = "https://auth.example.com/realms/DatingApp"
                })
                .Build();

            var controller = new PublicKeyController(config, new NullLogger<PublicKeyController>());

            var result = controller.Get().Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(StatusCodes.Status410Gone);

            var payload = result.Value!;
            var jwks = payload.GetType().GetProperty("jwks")?.GetValue(payload) as string;
            jwks.Should().Be("https://auth.example.com/realms/DatingApp/protocol/openid-connect/certs");
        }

        [Fact]
        public void Get_UsesFallbackAuthority_WhenNotConfigured()
        {
            var controller = new PublicKeyController(new ConfigurationBuilder().Build(), new NullLogger<PublicKeyController>());

            var result = controller.Get().Should().BeOfType<ObjectResult>().Subject;
            result.StatusCode.Should().Be(StatusCodes.Status410Gone);

            var payload = result.Value!;
            var jwks = payload.GetType().GetProperty("jwks")?.GetValue(payload) as string;
            jwks.Should().Be("https://auth.yourdatingapp.com/realms/DatingApp/protocol/openid-connect/certs");
        }
    }
}