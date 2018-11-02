using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Services;
using Crossroads.Service.Auth.Factories;
using Microsoft.Extensions.Logging;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Common.Security;
using Crossroads.Service.Auth.Models;
using Crossroads.Service.Auth.Exceptions;
using Microsoft.IdentityModel.Tokens;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api")]
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

        /// <summary>
        /// Get the authentication and authorization information associated with a bearer access token.
        /// </summary>
        /// <returns>Returns AuthDTO</returns>
        /// <param name="Authorization">Authorization is an oauth2 access token.</param>
        /// TODO: Add a description for authorization parameter
        [HttpGet("authorize")]
        public async Task<ActionResult<AuthDTO>> Get([FromHeader] string Authorization)
        {
            try
            {
                AuthDTO authDTO = await AuthService.Authorize(Authorization,
                                                  _configurationFactory,
                                                  _apiUserRepository,
                                                  _authenticationRepository,
                                                  _ministryPlatformRestRequestBuilder);

                return authDTO;
            }
            catch (TokenMalformedException ex)
            {
                //Token was malformatted - missing characters, modified, etc.
                return StatusCode(400, ex.Message);
            }
            catch (SecurityTokenInvalidIssuerException ex)
            {
                //The issuer of the token was invalid - we are expecting okta or mp
                return StatusCode(400, ex.Message);
            }
            catch (SecurityTokenValidationException ex)
            {
                //This is a generic exception that will catch either:
                //1. Token is expired or not yet valid
                //2. Token signing keys are not valid
                return StatusCode(400, ex.Message);
            }
            catch (NoContactIdAvailableException ex)
            {
                //Token was valid but we can't find an mp contact id
                return StatusCode(400, ex.Message);
            }
        }
    }
}