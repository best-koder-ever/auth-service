using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks; 
using AuthService.DTOs;
using AuthService.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Logging;
using AuthService.DTOs;
using System.Linq;


namespace AuthService.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <param name="dto">The registration details.</param>
    /// <returns>A success message and a token if registration is successful.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(new { message = "User registered successfully.", token = result });
        }
        catch (ApplicationException ex)
        {
            if (ex.Message.Contains("Email already exists"))
            {
                return BadRequest(new { error = "Email already exists." });
            }
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <param name="dto">The login details.</param>
    /// <returns>A JWT token if authentication is successful.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // DEMO_MODE: allow demo logins on /api/auth/login
        var demoMode = Environment.GetEnvironmentVariable("DEMO_MODE");
        if (string.Equals(demoMode, "true", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                // Look up seeded user in DB (in-memory)
                var user = await _authService.GetUserByEmailAsync(dto.Email);
                if (user == null || !await _authService.CheckPasswordAsync(user, dto.Password))
                {
                    return Unauthorized(new { error = "Demo user not found or password incorrect." });
                }
                var token = _authService.GenerateJwtToken(user);
                var response = new AuthService.DTOs.AuthResponseDto
                {
                    Success = true,
                    Message = "Demo login successful",
                    UserId = int.TryParse(user.Id, out var uid) ? uid : 0,
                    Email = user.Email,
                    Token = token,
                    RefreshToken = GenerateDemoRefreshToken(),
                    ExpiresAt = DateTime.UtcNow.AddHours(24),
                    UserProfile = new AuthService.DTOs.UserProfileSummaryDto
                    {
                        Id = int.TryParse(user.Id, out var uid2) ? uid2 : 0,
                        Name = user.UserName,
                        Email = user.Email,
                        IsVerified = true,
                        IsOnline = true,
                        LastActiveAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    }
                };
                _logger.LogInformation($"Demo login successful for {dto.Email}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in demo login");
                return StatusCode(500, "Demo login error");
            }
        }

        // ...existing code...
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(new { token = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized login attempt: {Message}", ex.Message);
            return Unauthorized(new { error = "Invalid credentials." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during login.");
            return StatusCode(500, new { error = "An unexpected error occurred." });
        }
    }

    // Helper methods for demo login
    private string GenerateDemoToken(int userId, string email)
    {
        var header = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"));
        var payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{{\"sub\":\"{userId}\",\"email\":\"{email}\",\"exp\":{DateTimeOffset.UtcNow.AddHours(24).ToUnixTimeSeconds()}}}"));
        var signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("demo_signature"));
        return $"{header}.{payload}.{signature}";
    }
    private string GenerateDemoRefreshToken()
    {
        return $"demo_refresh_{Guid.NewGuid():N}";
    }
    private string GetDemoNameFromEmail(string email)
    {
        var localPart = email.Split('@')[0];
        return localPart.Split('.').Length > 1 
            ? string.Join(" ", localPart.Split('.').Select(part => char.ToUpper(part[0]) + part.Substring(1)))
            : char.ToUpper(localPart[0]) + localPart.Substring(1);
    }

    /// <summary>
    /// Logs in a user using a Facebook access token.
    /// </summary>
    /// <param name="accessToken">The Facebook access token.</param>
    /// <returns>A JWT token if login is successful.</returns>
    [HttpPost("login/facebook")]
    public async Task<IActionResult> LoginWithFacebook([FromBody] string accessToken)
    {
        var result = await _authService.LoginWithFacebookAsync(accessToken);
        return Ok(new { token = result });
    }

    /// <summary>
    /// Logs in a user using a Google ID token.
    /// </summary>
    /// <param name="idToken">The Google ID token.</param>
    /// <returns>A JWT token if login is successful.</returns>
    [HttpPost("login/google")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] string idToken)
    {
        var result = await _authService.LoginWithGoogleAsync(idToken);
        return Ok(new { token = result });
    }

    /// <summary>
    /// Logs in a user using a phone number and verification code.
    /// </summary>
    /// <param name="dto">The phone number and verification code.</param>
    /// <returns>A JWT token if login is successful.</returns>
    [HttpPost("login/phone")]
    public async Task<IActionResult> LoginWithPhoneNumber([FromBody] PhoneNumberLoginDto dto)
    {
        var result = await _authService.LoginWithPhoneNumberAsync(dto.PhoneNumber, dto.Code);
        return Ok(new { token = result });
    }

    /// <summary>
    /// Validates a JWT token.
    /// </summary>
    /// <param name="token">The JWT token to validate.</param>
    /// <returns>A success message if the token is valid.</returns>
    [HttpPost("validate")]
    [Consumes("text/plain")]
    public IActionResult ValidateToken([FromBody] string token)
    {
        try
        {
            Console.WriteLine($"Received Content-Type: {Request.ContentType}");
            Console.WriteLine($"Received Token: {token}");

            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { error = "Token is required." });
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("SuperSecretKey12345678901234567890"); // Match the test key
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "TestIssuer",
                ValidAudience = "TestAudience",
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            return Ok(new { message = "Token is valid." });
        }
        catch (SecurityTokenMalformedException ex)
        {
            _logger.LogWarning(ex, "Malformed token received.");
            return Unauthorized(new { error = "Malformed token.", details = ex.Message });
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(new { error = "Invalid token.", details = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in ValidateToken endpoint.");
            return StatusCode(500, new { error = "An error occurred while validating the token.", details = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class PublicKeyController : ControllerBase
{
    [HttpGet]
    public IActionResult GetPublicKey()
    {
        var publicKey = System.IO.File.ReadAllText("public.key");
        return Ok(publicKey);
    }
}
}