using Crossroads.Service.Auth.Constants;
using Crossroads.Web.Auth.Models;
using Microsoft.IdentityModel.Tokens;
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

        public UserInfo GetUserInfo(string originalToken,
                                    string impersonateUserId,
                                    CrossroadsDecodedToken crossroadsDecodedToken,
                                    string mpAPIToken)
        {
            UserInfo userInfoObject = new UserInfo();
            int contactId = GetContactIdFromToken(originalToken, crossroadsDecodedToken);

            if (impersonateUserId != null)
            {
                // Check to see if the original token has the impersonateUser property
                // If they don't return an error about trying to impersonate when they aren't allowed
                // If they do then lets get the contact id for the user they are trying to impersonate instead
            }

            userInfoObject.Mp = _mpUserService.GetMpUserInfoFromContactId(contactId, mpAPIToken);

            return userInfoObject;
        }

        public Authorization GetAuthorizations(CrossroadsDecodedToken crossroadsDecodedToken, string mpAPIToken, int mpContactId)
        {
            Authorization authorizationObject = new Authorization();

            if (crossroadsDecodedToken.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
            {
                authorizationObject.OktaRoles = _oktaUserService.GetRoles(crossroadsDecodedToken);
            }

            authorizationObject.MpRoles = _mpUserService.GetRoles(mpAPIToken, mpContactId);

            return authorizationObject;
        }

        private bool UserCanImpersonate(string username)
        {
            
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
