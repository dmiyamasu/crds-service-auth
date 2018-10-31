using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Services;
using Crossroads.Service.Auth.Factories;
using Microsoft.Extensions.Logging;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using Crossroads.Service.Auth.Models;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string AuthHeaderKey = "authorization";
        private readonly OIDConfigurationFactory _configurationFactory;
        private readonly ILogger<AuthController> _logger;
        private readonly IApiUserRepository _apiUserRepository;
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IMinistryPlatformRestRequestBuilderFactory _ministryPlatformRestRequestBuilder;

        public AuthController(OIDConfigurationFactory configurationFactory, 
                              IApiUserRepository apiUserRepository,
                              IMinistryPlatformRestRequestBuilderFactory ministryPlatformRestRequestBuilder,
                              IAuthenticationRepository authenticationRepository,
                              ILogger<AuthController> logger)
        {
            _configurationFactory = configurationFactory;
            _logger = logger;
            _apiUserRepository = apiUserRepository;
            _ministryPlatformRestRequestBuilder = ministryPlatformRestRequestBuilder;
            _authenticationRepository = authenticationRepository;
        }

        // GET api/auth
        [HttpGet]
        public async Task<ActionResult<AuthDTO>> Get([FromHeader] string Authorization)
        {
            return await AuthService.IsAuthorized(Authorization,
                                                  _configurationFactory,
                                                  _apiUserRepository,
                                                  _authenticationRepository,
                                                  _ministryPlatformRestRequestBuilder);
        }
    }
}
