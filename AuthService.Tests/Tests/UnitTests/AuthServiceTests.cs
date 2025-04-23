using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace AuthService.Tests.UnitTests
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<IKeyProvider> _keyProviderMock;
        private readonly AuthService.Services.AuthService _authService;

        public AuthServiceTests()
        {
            // Mock UserManager<User>
            _userManagerMock = new Mock<UserManager<User>>(
                Mock.Of<IUserStore<User>>(),
                null, null, null, null, null, null, null, null
            );

            // Mock SignInManager<User>
            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                Mock.Of<IHttpContextAccessor>(), // Mock IHttpContextAccessor
                Mock.Of<IUserClaimsPrincipalFactory<User>>(),
                null, null, null, null
            );

            // Mock IConfiguration
            _configurationMock = new Mock<IConfiguration>();

            // Mock IKeyProvider
            _keyProviderMock = new Mock<IKeyProvider>();

            // Create an instance of AuthService with mocked dependencies
            _authService = new AuthService.Services.AuthService(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _configurationMock.Object,
                _keyProviderMock.Object
            );
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "password123"
            };

            var user = new User { Email = loginDto.Email, Id = "123" };

            // Mock UserManager to return a user when FindByEmailAsync is called
            _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);

            // Mock UserManager to return true for CheckPasswordAsync
            _userManagerMock.Setup(um => um.CheckPasswordAsync(user, loginDto.Password))
                .ReturnsAsync(true);

            // Mock IConfiguration to return a test issuer
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");

            // Generate a mock RSA key
            var rsa = RSA.Create();
            rsa.ImportFromPem(@"-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCKY3lndzhDBY4k
1YZbYeUu/lENGqFamFf9CwyAHzQ0sH2sro0sUta71OBorNEm9xjYfcQvDAC+49IS
cxWVo40IEEjdR7vYrKNr5WbghC2lmjm4dhnExns8x0B4TSQ/JfmpcMBlz2Yfz4hh
Y1Ev1Hv7pbaRrlUhAOB+hqSQulLxzoLtJZkcvVgxm24A2Mv788qbrBeGVLiY/31B
eus7yzxP40Jts9BFlq8elF4dlKzhZtRYHnFJCMWGSzQv3zuWQvm2JpAme+ACFG9V
uSjeH6b82D/DGwtzskZoTuMy+Tmeq89JqQ7M2EMkN8fKJIEvepOHHC9RglB7/5bN
3xeMYWGPAgMBAAECggEAOJGoD1zDEVaEf9cOyG0Qd7C6pf4kRfmvQf9RuU/IEd+x
R7TEfYFRSM+geflmt6RbMifa2ZZw+Zb8CNGlWZlU7Z5mgpmvlkcfuFu6PX2agS61
eItCRlcSCkqytlpmPjACSbqO0y8/4jH94D1ucvxLQBdBtXRLVSXcHqxgOc8IV9JG
ZpgsjGSwezZ2zLVkqvsXZZXNmosfg+rxQuycMD+j9DprMa30s6sXtZ5ZOs+ibOBN
b4C949co50S6PtUVERWupFb99qwbCAMOgOXefAdQULDsATWg4iW8wi+QPMMZF0lX
ytWYZLmIyVQPO5E8Dn5ppg/385KzgmSF5J/gAGNWlQKBgQDC2b/9aIzIeOSCrHCf
0TcILKGNzPvmBIo+GF+c4jYeEQqInmU369ZPnDkHziQ0O7uHAH1dTFdPgFVNWcmE
qAe92CJjq0E3nW//Lm/yD9+/CUaFFrgS5FX/+PhA4iSAV3pUOzFIoCs2d291eKdc
lzn+n2BvwVpRhGnFOC8eOKikCwKBgQC10ZRRBfGRz3o2+l5dT048dkN5upAsz9gT
6BzlY2wI+GkghD0SEZigSr7EwvjzzL3LdFfe5x6+5dxaiOXkgtMUvq5THpGiZ7pj
Vt3PjKoAuuUuAhB0QnKuFuKZ7CfM0jlSFCoQDHuI+v/sJjl07MeuyugaLWTJGpuL
diUzZDZHDQKBgQCScSMpKjV8ydc8Gqu+gXfxzdFRiHjdZBYeGyVo/F6d9ELNcPYz
tCzqwkfehOCS3T3Qdd7CiwinuJTjwJKC/+JpnRIjhGdMjCfLSrRZ4fJQWoFEr1GC
6Vd1PUIfSZcTWiuXOLGOmso/cj4ztI1cOlAc/N12wIPH9lOkJNjMxtqABwKBgQC1
F4CNTskzvJ3ywl5Yu9Ol3vkH9m0BZSbHdQnK48LIEHvM2kllhMcq6CeoHLYPRh7H
1SJsLnDuHE3kkrO/bRpGcEF7IlhVlNENfojA3064GW6I659t3H0SrlKWkqN1mvFi
shjPEU+9uJpMoCncLrYYf5q77/iRYQIJ3uvgivCQxQKBgD5NZjHPIXZpFMCvqWRN
TokyQ+tf3lqoBEGfd4iY0zYF6UIxW3BhbhTJo1hfutmS0Ly+wgBV4r/Deba4Q8mQ
Or+g23l5l4kp2imca60ayG/GVNT2I9Iy3Z++w3KFaW1YhvNmUwIKRLpYmkjlJEtP
m2a/NKF5Zyi1ZuVAr7VkMgw+
-----END PRIVATE KEY-----");

            // Mock IKeyProvider to return the mock RSA key
            _keyProviderMock.Setup(kp => kp.GetPrivateKey()).Returns(rsa);

            // Act
            var token = await _authService.LoginAsync(loginDto);

            // Assert
            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_ShouldThrowUnauthorizedAccessException_WhenCredentialsAreInvalid()
        {
            // Arrange
            var loginDto = new LoginDto
            {
                Email = "invalid@example.com",
                Password = "wrongpassword"
            };

            // Mock UserManager to return null when FindByEmailAsync is called
            _userManagerMock.Setup(um => um.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((User?)null); // Explicitly mark as nullable

            // Act
            Func<Task> act = async () => await _authService.LoginAsync(loginDto);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Invalid credentials");
        }

        [Fact]
        public void GenerateJwtToken_ShouldReturnValidToken_WhenPrivateKeyIsMocked()
        {
            // Arrange
            var user = new User { Email = "test@example.com", Id = "123" };

            // Generate a mock RSA key
            var rsa = RSA.Create();
            rsa.ImportFromPem(@"-----BEGIN PRIVATE KEY-----
MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCKY3lndzhDBY4k
1YZbYeUu/lENGqFamFf9CwyAHzQ0sH2sro0sUta71OBorNEm9xjYfcQvDAC+49IS
cxWVo40IEEjdR7vYrKNr5WbghC2lmjm4dhnExns8x0B4TSQ/JfmpcMBlz2Yfz4hh
Y1Ev1Hv7pbaRrlUhAOB+hqSQulLxzoLtJZkcvVgxm24A2Mv788qbrBeGVLiY/31B
eus7yzxP40Jts9BFlq8elF4dlKzhZtRYHnFJCMWGSzQv3zuWQvm2JpAme+ACFG9V
uSjeH6b82D/DGwtzskZoTuMy+Tmeq89JqQ7M2EMkN8fKJIEvepOHHC9RglB7/5bN
3xeMYWGPAgMBAAECggEAOJGoD1zDEVaEf9cOyG0Qd7C6pf4kRfmvQf9RuU/IEd+x
R7TEfYFRSM+geflmt6RbMifa2ZZw+Zb8CNGlWZlU7Z5mgpmvlkcfuFu6PX2agS61
eItCRlcSCkqytlpmPjACSbqO0y8/4jH94D1ucvxLQBdBtXRLVSXcHqxgOc8IV9JG
ZpgsjGSwezZ2zLVkqvsXZZXNmosfg+rxQuycMD+j9DprMa30s6sXtZ5ZOs+ibOBN
b4C949co50S6PtUVERWupFb99qwbCAMOgOXefAdQULDsATWg4iW8wi+QPMMZF0lX
ytWYZLmIyVQPO5E8Dn5ppg/385KzgmSF5J/gAGNWlQKBgQDC2b/9aIzIeOSCrHCf
0TcILKGNzPvmBIo+GF+c4jYeEQqInmU369ZPnDkHziQ0O7uHAH1dTFdPgFVNWcmE
qAe92CJjq0E3nW//Lm/yD9+/CUaFFrgS5FX/+PhA4iSAV3pUOzFIoCs2d291eKdc
lzn+n2BvwVpRhGnFOC8eOKikCwKBgQC10ZRRBfGRz3o2+l5dT048dkN5upAsz9gT
6BzlY2wI+GkghD0SEZigSr7EwvjzzL3LdFfe5x6+5dxaiOXkgtMUvq5THpGiZ7pj
Vt3PjKoAuuUuAhB0QnKuFuKZ7CfM0jlSFCoQDHuI+v/sJjl07MeuyugaLWTJGpuL
diUzZDZHDQKBgQCScSMpKjV8ydc8Gqu+gXfxzdFRiHjdZBYeGyVo/F6d9ELNcPYz
tCzqwkfehOCS3T3Qdd7CiwinuJTjwJKC/+JpnRIjhGdMjCfLSrRZ4fJQWoFEr1GC
6Vd1PUIfSZcTWiuXOLGOmso/cj4ztI1cOlAc/N12wIPH9lOkJNjMxtqABwKBgQC1
F4CNTskzvJ3ywl5Yu9Ol3vkH9m0BZSbHdQnK48LIEHvM2kllhMcq6CeoHLYPRh7H
1SJsLnDuHE3kkrO/bRpGcEF7IlhVlNENfojA3064GW6I659t3H0SrlKWkqN1mvFi
shjPEU+9uJpMoCncLrYYf5q77/iRYQIJ3uvgivCQxQKBgD5NZjHPIXZpFMCvqWRN
TokyQ+tf3lqoBEGfd4iY0zYF6UIxW3BhbhTJo1hfutmS0Ly+wgBV4r/Deba4Q8mQ
Or+g23l5l4kp2imca60ayG/GVNT2I9Iy3Z++w3KFaW1YhvNmUwIKRLpYmkjlJEtP
m2a/NKF5Zyi1ZuVAr7VkMgw+
-----END PRIVATE KEY-----");

            // Mock IKeyProvider to return the mock RSA key
            _keyProviderMock.Setup(kp => kp.GetPrivateKey()).Returns(rsa);

            // Mock IConfiguration to return a test issuer and audience
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            // Act
            var token = _authService.GenerateJwtToken(user);

            // Assert
            token.Should().NotBeNullOrEmpty();
            token.Should().Contain(".");
        }
    }
}