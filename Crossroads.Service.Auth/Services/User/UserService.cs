using Crossroads.Service.Auth.Constants;
using Crossroads.Service.Auth.Models;
using Microsoft.IdentityModel.Tokens;
using static Crossroads.Service.Auth.Services.JwtService;
using Crossroads.Service.Auth.Interfaces;

namespace Crossroads.Service.Auth.Services
{
    public class UserService : IUserService
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private IMpUserService _mpUserService;
        private IOktaUserService _oktaUserService;

        public UserService(IMpUserService mpUserService, IOktaUserService oktaUserService)
        {
            _mpUserService = mpUserService;
            _oktaUserService = oktaUserService;
        }

        public UserInfoDTO GetUserInfo(string originalToken,
                                       CrossroadsDecodedToken crossroadsDecodedToken,
                                       string mpAPIToken)
        {
            UserInfoDTO userInfoObject = new UserInfoDTO();
            int contactId = GetContactIdFromToken(originalToken, crossroadsDecodedToken);

            userInfoObject.Mp = _mpUserService.GetMpUserInfoFromContactId(contactId, mpAPIToken);

            return userInfoObject;
        }

        public AuthorizationDTO GetAuthorizations(CrossroadsDecodedToken crossroadsDecodedToken, string mpAPIToken, int mpContactId)
        {
            AuthorizationDTO authorizationObject = new AuthorizationDTO();

            if (crossroadsDecodedToken.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
            {
                authorizationObject.OktaRoles = _oktaUserService.GetRoles(crossroadsDecodedToken);
            }

            authorizationObject.MpRoles = _mpUserService.GetRoles(mpAPIToken, mpContactId);

            return authorizationObject;
        }

        private int GetContactIdFromToken(string originalToken, CrossroadsDecodedToken crossroadsDecodedToken)
        {
            int contactId = -1;

            if (crossroadsDecodedToken.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
            {
                contactId = _oktaUserService.GetMpContactIdFromDecodedToken(crossroadsDecodedToken);
            }
            else if (crossroadsDecodedToken.authProvider == AuthConstants.AUTH_PROVIDER_MP)
            {
                contactId = _mpUserService.GetMpContactIdFromToken(originalToken);
            }
            else
            {
                //This should never happen based on previous logic
                _logger.Error("Invalid issuer when there should not be an invalid issuer w/ token: " + originalToken);
                throw new SecurityTokenInvalidIssuerException();
            }

            return contactId;
        }
    }
}
