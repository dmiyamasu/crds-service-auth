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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET api/auth
        [HttpGet]
        public ActionResult<JwtSecurityToken> Get([FromHeader] string Authorization)
        {
            return _authService.IsAuthorized(Authorization);
        }
    }
}
