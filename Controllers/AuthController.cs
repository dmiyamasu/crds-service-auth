using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string AuthHeaderKey = "authorization";
        private readonly IAuthService _authService;
        private readonly IOIDConfigurationFactory _configurationFactory;

        public AuthController(IAuthService authService, IOIDConfigurationFactory configurationFactory)
        {
            _authService = authService;
            _configurationFactory = configurationFactory;
        }

        // GET api/auth
        [HttpGet]
        public async Task<ActionResult<JObject>> Get([FromHeader] string Authorization)
        {
            return await _authService.IsAuthorized(Authorization, _configurationFactory);
        }
    }
}
