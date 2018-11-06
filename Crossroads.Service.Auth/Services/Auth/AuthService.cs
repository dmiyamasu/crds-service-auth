using System.Threading.Tasks;
using Crossroads.Service.Auth.Configurations;
using Crossroads.Web.Common.MinistryPlatform;
using Crossroads.Service.Auth.Models;
using static Crossroads.Service.Auth.Services.JwtService;
using Crossroads.Service.Auth.Interfaces;

namespace Crossroads.Service.Auth.Services
{
    public class AuthService : IAuthService
    {
        //TODO: Create a base class that has a logger available
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private OIDConfigurations _configurations;
        private IUserService _userService;
        private IApiUserRepository _apiUserRepository;

        public AuthService(OIDConfigurations configurations,
                           IUserService userService,
                           IApiUserRepository apiUserRepository)
        {
            _configurations = configurations;
            _apiUserRepository = apiUserRepository;
            _userService = userService;
        }

        public async Task<AuthDTO> GetAuthorization(string token)
        {
            CrossroadsDecodedToken decodedToken = await DecodeAndValidateToken(token, _configurations);

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