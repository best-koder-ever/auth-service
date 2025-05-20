using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous] // Disable authorization for testing
        [HttpGet]
        public IActionResult Get()
        {
            _logger.LogInformation("This is an information log.");
            _logger.LogWarning("This is a warning log.");
            _logger.LogError("This is an error log.");
            return Ok("Logs have been generated.");
        }
    }
}