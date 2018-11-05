using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Crossroads.Service.Auth.Constants;
using Crossroads.Service.Auth.Configurations;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly OIDConfigurations _configurations;

        public HealthController(OIDConfigurations configurations)
        {
            _configurations = configurations;
        }

        // GET api/auth
        [HttpGet("ready")]
        [HttpGet("live")]
        public async Task<ActionResult<JObject>> Get()
        {
            await _configurations.mpConfigurationManager.GetConfigurationAsync();
            await _configurations.oktaConfigurationManager.GetConfigurationAsync();

            return new JObject();
        }
    }
}
