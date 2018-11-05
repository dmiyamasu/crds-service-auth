using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Crossroads.Service.Auth.Models;
using Crossroads.Service.Auth.Exceptions;
using Microsoft.IdentityModel.Tokens;
using Crossroads.Service.Auth.Interfaces;

namespace Crossroads.Service.Auth.Controllers
{
    [Route("api")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private const string AuthHeaderKey = "authorization";
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                AuthDTO authDTO = await _authService.Authorize(Authorization);

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
                //All other exceptions are logged when the exception is thrown
                _logger.Debug(ex.Message);
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
