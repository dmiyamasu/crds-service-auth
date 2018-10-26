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
        internal static async Task<JObject> IsAuthorized(string token, 
                                                         OIDConfigurationFactory configurationFactory, 
                                                         IApiUserRepository apiUserRepository,
                                                         IAuthenticationRepository authenticationRepository,
                                                         IMinistryPlatformRestRequestBuilderFactory mpRestBuilder)
        {
            DecodeTokenResponse decodeTokenResponse = await DecodeToken(token, configurationFactory);

            JObject authenticationObject = buildAuthenticationResponseObject(decodeTokenResponse);
            JObject authorizationObject = buildAuthorizationResponseObject(decodeTokenResponse);

            JObject responseObject = new JObject();

            responseObject["authentication"] = authenticationObject;
            responseObject["authorization"] = authorizationObject;

            return responseObject;
        }

        private static JObject buildAuthorizationResponseObject(DecodeTokenResponse decodeTokenResponse, 
                                                                IAuthenticationRepository authenticationRepository)
        {
            JObject authorizationObject = new JObject();

            if (decodeTokenResponse.decodedToken != null)
            {
                int contactId = 0;

                // If okta token:
                if (decodeTokenResponse.authProvider == AuthConstants.AUTH_PROVIDER_OKTA)
                {
                    // try to pull contact and/or userId from token
                    // try to get claims
                }
                else // If mp token:
                {
                    // Go get the contact and/or userId
                    contactId = authenticationRepository.GetContactId(token);
                }

                // Go get the roles from mp
                var mpAPIToken = apiUserRepository.GetDefaultApiClientToken();
                var roles = mpRestBuilder.NewRequestBuilder()
                  .WithAuthenticationToken(token)
                  .AddSelectColumn("Role_ID")
                  .WithFilter($"User_ID_Table_Contact_ID_Table.[Contact_ID]={contactId}")
                  .Build()
                  .Search<JObject>("dp_User_Roles");
                var rolesList = roles.Select(r => r.Value<int>("Role_ID"));
            }

            return authorizationObject();
        }

        private static JObject buildAuthenticationResponseObject(DecodeTokenResponse decodeTokenResponse)
        {
            JObject authenticationObject = new JObject();

            if (decodeTokenResponse.decodedToken != null)
            {
                authenticationObject["authenticated"] = true;
                authenticationObject["provider"] = decodeTokenResponse.authProvider;
                authenticationObject["message"] = decodeTokenResponse.authFailureReason;
                //tokenValidResponse["claims"] = new List<string>();
            }
            else
            {
                authenticationObject["authenticated"] = false;
                authenticationObject["provider"] = decodeTokenResponse.authProvider;
                authenticationObject["message"] = decodeTokenResponse.authFailureReason;
                //tokenValidResponse["claims"] = new List<string>();
            }

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