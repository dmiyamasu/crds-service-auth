using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Interfaces;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IOIDConfigurationService _configService;

        public HealthController(IOIDConfigurationService configService)
        {
            _configService = configService;
        }

        // GET api/auth
        [HttpGet("ready")]
        [HttpGet("live")]
        public async Task<ActionResult<JObject>> Get()
        {
            await _configService.GetMpConfigAsync();
            await _configService.GetOktaConfigAsync();

            return new JObject();
        }
    }
}
