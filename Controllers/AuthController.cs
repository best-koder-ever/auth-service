using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        private IActionResult EndpointRetired(string endpoint)
        {
            _logger.LogWarning("Legacy auth endpoint {Endpoint} was invoked after Keycloak migration.", endpoint);
            return StatusCode(StatusCodes.Status410Gone, new
            {
                message = "Legacy authentication endpoints have been retired. Use the Keycloak OpenID Connect flow instead.",
                endpoint,
                documentation = "https://docs.yourdatingapp.com/keycloak-migration"
            });
        }

        [HttpPost("register")]
        public IActionResult Register() => EndpointRetired("POST /api/auth/register");

        [HttpPost("login")]
        public IActionResult Login() => EndpointRetired("POST /api/auth/login");

        [HttpPost("login/facebook")]
        public IActionResult LoginWithFacebook() => EndpointRetired("POST /api/auth/login/facebook");

        [HttpPost("login/google")]
        public IActionResult LoginWithGoogle() => EndpointRetired("POST /api/auth/login/google");

        [HttpPost("login/phone")]
        public IActionResult LoginWithPhoneNumber() => EndpointRetired("POST /api/auth/login/phone");

        [HttpPost("refresh")]
        public IActionResult Refresh() => EndpointRetired("POST /api/auth/refresh");

        [HttpPost("logout")]
        public IActionResult Logout() => EndpointRetired("POST /api/auth/logout");

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword() => EndpointRetired("POST /api/auth/forgot-password");

        [HttpPost("verify-email")]
        public IActionResult VerifyEmail() => EndpointRetired("POST /api/auth/verify-email");

        [HttpPost("validate")]
        public IActionResult ValidateToken() => EndpointRetired("POST /api/auth/validate");
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PublicKeyController : ControllerBase
    {
        private readonly ILogger<PublicKeyController> _logger;
        private readonly string _authority;

        public PublicKeyController(IConfiguration configuration, ILogger<PublicKeyController> logger)
        {
            _logger = logger;
            _authority = configuration["Authentication:Keycloak:Authority"] ?? "https://auth.yourdatingapp.com/realms/DatingApp";
        }

        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("Legacy public key endpoint requested after Keycloak migration.");
            return StatusCode(StatusCodes.Status410Gone, new
            {
                message = "AuthService no longer distributes RSA keys. Use Keycloak's JWKS endpoint instead.",
                jwks = $"{_authority.TrimEnd('/')}/protocol/openid-connect/certs"
            });
        }
    }
}