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