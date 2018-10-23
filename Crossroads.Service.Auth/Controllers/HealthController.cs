using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Crossroads.Service.Auth.Constants;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IOIDConfigurationFactory _configurationFactory;

        public HealthController(ILogger<HealthController> logger, IOIDConfigurationFactory configurationFactory)
        {
            _logger = logger;
            _configurationFactory = configurationFactory;
        }



        // GET api/auth
        [HttpGet("ready")]
        [HttpGet("live")]
        public async Task<ActionResult<JObject>> Get()
        {
            await _configurationFactory.GetConfiguration(AuthConstants.AUTH_PROVIDER_MP).GetConfigurationAsync();
            await _configurationFactory.GetConfiguration(AuthConstants.AUTH_PROVIDER_OKTA).GetConfigurationAsync();

            return new JObject();
        }
    }
}
