using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(new { token = result });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(new { token = result });
    }

    [HttpPost("login/facebook")]
    public async Task<IActionResult> LoginWithFacebook([FromBody] string accessToken)
    {
        var result = await _authService.LoginWithFacebookAsync(accessToken);
        return Ok(new { token = result });
    }

    [HttpPost("login/google")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] string idToken)
    {
        var result = await _authService.LoginWithGoogleAsync(idToken);
        return Ok(new { token = result });
    }

    [HttpPost("login/phone")]
    public async Task<IActionResult> LoginWithPhoneNumber([FromBody] PhoneNumberLoginDto dto)
    {
        var result = await _authService.LoginWithPhoneNumberAsync(dto.PhoneNumber, dto.Code);
        return Ok(new { token = result });
    }
}