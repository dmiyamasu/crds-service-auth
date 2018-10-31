using System;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Utilities;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Crossroads.Service.Auth.Constants;
using Crossroads.Service.Auth.Factories;
using Crossroads.Web.Common.MinistryPlatform;
using System.Linq;
using Crossroads.Web.Common.Security;
using Crossroads.Service.Auth.Models;
using MinistryPlatform.Models;

namespace Crossroads.Service.Auth.Services
{
    public static class AuthService
    {
        struct DecodeTokenResponse
        {
            public JwtSecurityToken decodedToken;
            public string authFailureReason;
            public string authProvider;
        }

        //TODO: MP MIGRATION: This function currently checks for an mp token before okta. Validating the first token
        // is faster than validating the second. Once mp tokens are the minority of tokens being passed 
        // we should swap the order
        internal static async Task<AuthDTO> IsAuthorized(string token, 
                                                         OIDConfigurationFactory configurationFactory, 
                                                         IApiUserRepository apiUserRepository,
                                                         IAuthenticationRepository authenticationRepository,
                                                         IMinistryPlatformRestRequestBuilderFactory mpRestBuilder)
        {
            DecodeTokenResponse decodeTokenResponse = await DecodeToken(token, configurationFactory);

            AuthenticationDTO authenticationObject = buildAuthenticationResponseObject(decodeTokenResponse);
            UserInfoDTO userInfoObject = buildUserInfoObject(token,
                                                        decodeTokenResponse,
                                                         authenticationRepository,
                                                             apiUserRepository,
                                                            mpRestBuilder);
            AuthorizationDTO authorizationObject = buildAuthorizationResponseObject(token,
                                                                           decodeTokenResponse,
                                                                                    authenticationRepository,
                                                                            //userInfoObject.contactId,
                                                                          apiUserRepository,
                                                                          mpRestBuilder);

            AuthDTO responseObject = new AuthDTO();

            responseObject.authentication = authenticationObject;
            responseObject.authorization = authorizationObject;
            responseObject.userInfo = userInfoObject;

            return responseObject;
        }

        private static UserInfoDTO buildUserInfoObject(string originalToken,
                                                   DecodeTokenResponse decodeTokenResponse,
                                                   IAuthenticationRepository authenticationRepository,
                                                       IApiUserRepository userRepository,
                                                               IMinistryPlatformRestRequestBuilderFactory ministryPlatformRest)
        {
            UserInfoDTO userInfoObject = new UserInfoDTO();
            int contactId = -1;

            if (decodeTokenResponse.decodedToken != null)
            {
                // If okta token:
                if (decodeTokenResponse.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
                {
                    //TODO:
                    // try to pull contact and/or userId from token
                    // try to get claims
                }
                else // If mp token:
                {
                    // Go get the contact and/or userId
                    contactId = authenticationRepository.GetContactId(originalToken);
                }

                //TODO: Consider popping this out into its own function to buildMpUserInfo
                if (contactId > 0)
                {
                    var mpAPIToken = userRepository.GetDefaultApiClientToken();

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

                    userInfoObject.mpUserId = contact.UserAccount;
                    userInfoObject.mpParticipantId = contact.ParticipantRecord;
                    userInfoObject.mpHouseholdId = contact.HouseholdId;
                    userInfoObject.mpEmail = contact.EmailAddress;
                    userInfoObject.mpDonorId = contact.DonorRecord;
                }
            }

            userInfoObject.mpContactId = contactId;

            return userInfoObject;
        }

        private static AuthorizationDTO buildAuthorizationResponseObject(string originalToken,
                                                                DecodeTokenResponse decodeTokenResponse, 
                                                                         IAuthenticationRepository authenticationRepository,
                                                               IApiUserRepository userRepository,
                                                               IMinistryPlatformRestRequestBuilderFactory ministryPlatformRest)
        {
            AuthorizationDTO authorizationObject = new AuthorizationDTO();

            if (decodeTokenResponse.decodedToken != null)
            {
                // If okta token:
                if (decodeTokenResponse.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
                {
                    // try to get claims
                }

                // Go get the roles from mp

                //TODO: DECIDE WHAT FORMAT WE WANT

                //This gets the role IDS
                //var mpAPIToken = userRepository.GetDefaultApiClientToken();
                //var roles = ministryPlatformRest.NewRequestBuilder()
                //  .WithAuthenticationToken(mpAPIToken)
                //  .AddSelectColumn("Role_ID")
                //  .WithFilter($"User_ID_Table_Contact_ID_Table.[Contact_ID]={contactId}")
                //  .Build()
                //  .Search<JObject>("dp_User_Roles");
                //var rolesList = roles.Select(r => r.Value<int>("Role_ID"));

                //This gets the Role Names
                var rolesList = authenticationRepository.GetUserRolesFromToken(originalToken);

                authorizationObject.mpRoles = rolesList;
            }

            return authorizationObject;
        }

        private static AuthenticationDTO buildAuthenticationResponseObject(DecodeTokenResponse decodeTokenResponse)
        {
            AuthenticationDTO authenticationObject = new AuthenticationDTO();

            authenticationObject.authenticated = decodeTokenResponse.decodedToken != null;
            authenticationObject.provider = decodeTokenResponse.authProvider;
            authenticationObject.message = decodeTokenResponse.authFailureReason;

            return authenticationObject;
        }

        private static async Task<DecodeTokenResponse> DecodeToken(string token, OIDConfigurationFactory configurationFactory)
        {
            JwtSecurityToken decodedToken = null;
            string authFailureReason = "";
            string authProvider = AuthConstants.AUTH_PROVIDER_MP;

            //Get the configuration manager for the mp provider
            ConfigurationManager<OpenIdConnectConfiguration> configurationManager;
            configurationManager = configurationFactory.mpConfigurationManager;

            //try mp - if that doesn't work it must be okta
            try
            {
                decodedToken = await JwtUtilities.ValidateTokenAsync(token, configurationManager);
            }
            catch (SecurityTokenValidationException stvex)
            {
                //Check the specifics of the exception - if exception was a signature validation failure it may be an okta token
                // If so we will try to validate against okta config
                if (stvex.Message.Contains("IDX10501"))
                {
                    authProvider = AuthConstants.AUTH_PROVIDER_OKTA;
                    configurationManager = configurationFactory.oktaConfigurationManager;

                    //Otherwise try the okta token
                    try
                    {
                        decodedToken = await JwtUtilities.ValidateTokenAsync(token, configurationManager);
                    }
                    catch (SecurityTokenValidationException exception)
                    {
                        //TODO: Maybe log something?
                        authFailureReason = exception.Message;
                    }
                }
                //Otherwise just throw the error
                else
                {
                    //TODO: Maybe log something?
                    authFailureReason = stvex.Message;
                }
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Maybe log?
                authFailureReason = argex.Message;
            }

            DecodeTokenResponse response = new DecodeTokenResponse();
            response.authFailureReason = authFailureReason;
            response.authProvider = authProvider;
            response.decodedToken = decodedToken;

            return response;
        }
    }
}