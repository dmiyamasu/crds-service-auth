using System.Threading.Tasks;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Service.Auth.Models;
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

            // TODO: Consider using a Client API Token (ex: FRED)
            // TODO: Cache the token
            string mpAPIToken = _apiUserRepository.GetDefaultApiClientToken();

            UserInfoDTO userInfo = _userService.GetUserInfo(token, decodedToken, mpAPIToken);

            AuthorizationDTO authorizations = _userService.GetAuthorizations(decodedToken, mpAPIToken, userInfo.Mp.ContactId);

            AuthenticationDTO authentication = GetAuthentication(decodedToken);

            AuthDTO responseObject = new AuthDTO
            {
                Authentication = authentication,
                Authorization = authorizations,
                UserInfo = userInfo
            };

            return responseObject;
        }

        private static AuthenticationDTO GetAuthentication(CrossroadsDecodedToken decodeTokenResponse)
        {
            AuthenticationDTO authentication = new AuthenticationDTO();

            authentication.Provider = decodeTokenResponse.authProvider;

            return authentication;
        }
    }
}