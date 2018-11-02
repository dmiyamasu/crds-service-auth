using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using Crossroads.Service.Auth.Constants;
using Crossroads.Service.Auth.Factories;
using Crossroads.Web.Common.MinistryPlatform;
using System.Linq;
using Crossroads.Web.Common.Security;
using Crossroads.Service.Auth.Models;
using MinistryPlatform.Models;
using static Crossroads.Service.Auth.Services.JwtService;
using Crossroads.Service.Auth.Exceptions;

namespace Crossroads.Service.Auth.Services
{
    public static class AuthService
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        internal static async Task<AuthDTO> Authorize(string token, 
                                                         OIDConfigurationFactory configurationFactory, 
                                                         IApiUserRepository apiUserRepository,
                                                         IAuthenticationRepository authenticationRepository,
                                                         IMinistryPlatformRestRequestBuilderFactory mpRestBuilder)
        {
            CrossroadsDecodedToken decodedToken = await DecodeAndValidateToken(token, configurationFactory);

            var mpAPIToken = apiUserRepository.GetDefaultApiClientToken();
            UserInfoDTO userInfo = GetUserInfo(token,
                                               decodedToken,
                                               authenticationRepository,
                                               mpAPIToken,
                                               mpRestBuilder);
            
            AuthorizationDTO authorization = GetAuthorization(decodedToken,
                                                              userInfo,
                                                              mpAPIToken,
                                                              mpRestBuilder);

            AuthenticationDTO authentication = GetAuthentication(decodedToken);

            AuthDTO responseObject = new AuthDTO
            {
                Authentication = authentication,
                Authorization = authorization,
                UserInfo = userInfo
            };

            return responseObject;
        }

        private static UserInfoDTO GetUserInfo(string originalToken,
                                               CrossroadsDecodedToken crossroadsDecodedToken,
                                               IAuthenticationRepository authenticationRepository,
                                               string mpAPIToken,
                                               IMinistryPlatformRestRequestBuilderFactory ministryPlatformRest)
        {
            UserInfoDTO userInfoObject = new UserInfoDTO();
            int contactId = -1;

            // If okta token:
            if (crossroadsDecodedToken.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
            {
                //TODO:
                // try to pull contact and/or userId from token
                // try to get claims
            }
            else if (crossroadsDecodedToken.authProvider == AuthConstants.AUTH_PROVIDER_MP)
            {
                //TODO: See what happens when it can't find a contact Id
                contactId = authenticationRepository.GetContactId(originalToken);
            }
            else
            {
                //This should never happen based on previous logic
                _logger.Error("Invalid issuer when there should not be an invalid issuer w/ token: " + originalToken);
                throw new SecurityTokenInvalidIssuerException();
            }

            if (contactId > 0)
            {
                userInfoObject.Mp = GetMpUserInfo(contactId, mpAPIToken, ministryPlatformRest);
            }
            else
            {
                _logger.Error("No contactId Available for token: " + originalToken);
                throw new NoContactIdAvailableException();
            }

            return userInfoObject;
        }

        private static MpUserInfoDTO GetMpUserInfo(int contactId,
                                     string mpAPIToken,
                                     IMinistryPlatformRestRequestBuilderFactory ministryPlatformRest)
        {
            var columns = new string[] {
                    "User_Account",
                    "Donor_Record",
                    "Participant_Record",
                    "Email_Address",
                    "Household_ID"
                };

            var contact = ministryPlatformRest.NewRequestBuilder()
                                              .WithAuthenticationToken(mpAPIToken)
                                              .WithSelectColumns(columns)
                                              .Build()
                                              .Get<MpContact>(contactId);

            //TODO: What happens if anything fails?

            MpUserInfoDTO mpUserInfoDTO = new MpUserInfoDTO
            {
                ContactId = contactId,
                UserId = contact.UserAccount,
                ParticipantId = contact.ParticipantRecord,
                HouseholdId = contact.HouseholdId,
                Email = contact.EmailAddress,
                DonorId = contact.DonorRecord
            };

            return mpUserInfoDTO;
        }

        private static AuthorizationDTO GetAuthorization(CrossroadsDecodedToken crossroadsDecodedToken,
                                                         UserInfoDTO userInfo,
                                                         string mpAPIToken,
                                                         IMinistryPlatformRestRequestBuilderFactory ministryPlatformRest)
        {
            AuthorizationDTO authorizationObject = new AuthorizationDTO();

            // If okta token:
            if (crossroadsDecodedToken.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
            {
                // TODO: try to get claims

            }

            // Go get the roles from mp
            var columns = new string[] {
                    "dp_User_Roles.Role_ID",
                    "Role_ID_Table.Role_Name"
                };

            var roles = ministryPlatformRest.NewRequestBuilder()
                                            .WithAuthenticationToken(mpAPIToken)
                                            .WithSelectColumns(columns)
                                            .WithFilter($"User_ID_Table_Contact_ID_Table.[Contact_ID]={userInfo.Mp.ContactId}")
                                            .Build()
                                            .Search<JObject>("dp_User_Roles");
            
            var rolesDict = roles.ToDictionary(x => x.Value<int>("Role_ID"), x => x.Value<string>("Role_Name"));

            authorizationObject.MpRoles = rolesDict;

            return authorizationObject;
        }

        private static AuthenticationDTO GetAuthentication(CrossroadsDecodedToken decodeTokenResponse)
        {
            AuthenticationDTO authenticationObject = new AuthenticationDTO();

            authenticationObject.Authenticated = true;
            authenticationObject.Provider = decodeTokenResponse.authProvider;

            return authenticationObject;
        }
    }
}