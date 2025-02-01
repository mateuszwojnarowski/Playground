using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ILogger = Serilog.ILogger;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IdentityController : ControllerBase
    {
        private readonly ILogger _logger;

        public IdentityController(ILogger logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Authorize("ApiScope")]
        public IActionResult GetUserClaims()
        {
            _logger.Warning("Endpoint hit!");
            var userClaims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(userClaims);
        }

        [HttpGet("test")]
        public IActionResult GetTest()
        {
            _logger.Warning("Test endpoint hit!");
            return Ok("dupa");
        }
    }
}
