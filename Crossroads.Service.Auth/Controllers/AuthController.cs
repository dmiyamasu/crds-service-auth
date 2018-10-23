using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Interfaces;
using Microsoft.Extensions.Logging;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string AuthHeaderKey = "authorization";
        private readonly IAuthService _authService;
        private readonly IOIDConfigurationFactory _configurationFactory;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IOIDConfigurationFactory configurationFactory, ILogger<AuthController> logger)
        {
            _authService = authService;
            _configurationFactory = configurationFactory;
            _logger = logger;
        }

        // GET api/auth
        [HttpGet]
        public async Task<ActionResult<JObject>> Get([FromHeader] string Authorization)
        {
            return await _authService.IsAuthorized(Authorization, _configurationFactory);
        }
    }
}
