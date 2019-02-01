using System.Threading.Tasks;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Web.Auth.Models;
using Crossroads.Service.Auth.Interfaces;

namespace Crossroads.Service.Auth.Services
{
    public class AuthService : IAuthService
    {
        //TODO: Create a base class that has a logger available
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IOIDConfigurationService _configService;
        private IUserService _userService;
        private IApiUserRepository _apiUserRepository;
        private IJwtService _jwtService;

        public AuthService(IOIDConfigurationService configService,
                           IUserService userService,
                           IApiUserRepository apiUserRepository,
                           IJwtService jwtService)
        {
            _configService = configService;
            _apiUserRepository = apiUserRepository;
            _userService = userService;
            _jwtService = jwtService;
        }

        public async Task<AuthDTO> GetAuthorization(string token)
        {
            CrossroadsDecodedToken decodedToken = await _jwtService.DecodeAndValidateToken(token, _configService);

            UserInfo userInfo = null;
            Authorization authorizations = null;

            if (_jwtService.TokenIsOpenId(decodedToken))
            {
                // TODO: Consider using a Client API Token (ex: FRED)
                // TODO: Cache the token
                string mpAPIToken = _apiUserRepository.GetDefaultApiClientToken();

                userInfo = _userService.GetUserInfo(token, decodedToken, mpAPIToken);

                authorizations = _userService.GetAuthorizations(decodedToken, mpAPIToken, userInfo.Mp.ContactId);
            }
            else //This is not a user token so we can't get user info or user roles... Just allow it to happen I guess?
            {
                
            }

            Authentication authentication = GetAuthentication(decodedToken);

            AuthDTO responseObject = new AuthDTO
            {
                Authentication = authentication,
                Authorization = authorizations,
                UserInfo = userInfo
            };

            return responseObject;
        }

        private static Authentication GetAuthentication(CrossroadsDecodedToken decodeTokenResponse)
        {
            Authentication authentication = new Authentication();

            authentication.Provider = decodeTokenResponse.authProvider;

            return authentication;
        }
    }
}