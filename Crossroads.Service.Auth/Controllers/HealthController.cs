using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Crossroads.Service.Auth.Constants;
using Crossroads.Service.Auth.Factories;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly OIDConfigurationFactory _configurationFactory;

        public HealthController(ILogger<HealthController> logger, OIDConfigurationFactory configurationFactory)
        {
            _logger = logger;
            _configurationFactory = configurationFactory;
        }

        // GET api/auth
        [HttpGet("ready")]
        [HttpGet("live")]
        public async Task<ActionResult<JObject>> Get()
        {
            await _configurationFactory.mpConfigurationManager.GetConfigurationAsync();
            await _configurationFactory.oktaConfigurationManager.GetConfigurationAsync();

            return new JObject();
        }
    }
}
