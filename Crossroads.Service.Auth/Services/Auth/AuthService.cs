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
            _logger.Info("Decoding Token");
            CrossroadsDecodedToken decodedToken = await _jwtService.DecodeAndValidateToken(token, _configService);
            _logger.Info("Token Decoded");

            UserInfo userInfo = null;
            Authorization authorizations = null;

            _logger.Info("Getting API Token");
            string mpAPIToken = _apiUserRepository.GetDefaultApiClientToken();
            _logger.Info("API Token Retrieved");

            _logger.Info("Getting User Info");
            userInfo = _userService.GetUserInfo(token, decodedToken, mpAPIToken);
            _logger.Info("User Info Retrieved");

            _logger.Info("Getting Authorizations");
            authorizations = _userService.GetAuthorizations(decodedToken, mpAPIToken, userInfo.Mp.ContactId);
            _logger.Info("Authorizations Retrieved");

            _logger.Info("Getting Authentication");
            Authentication authentication = GetAuthentication(decodedToken);
            _logger.Info("Authentication Retrieved");

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