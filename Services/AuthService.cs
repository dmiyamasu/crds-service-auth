using System;
using System.Collections.Generic;
using System.Linq;
using Crossroads.Service.Auth.Interfaces;
using Newtonsoft.Json.Linq;
using Crossroads.Service.Auth.Utilities;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using Crossroads.Service.Auth.Constants;

namespace Crossroads.Service.Auth.Services
{
    public class AuthService : IAuthService
    {
        public AuthService() 
        {
            
        }

        //TODO: MP MIGRATION: This function currently checks for an mp token before okta. Validating the first token
        // is significatly faster than validating the second. Once mp tokens are the minority of tokens being passed 
        // we should swap the order
        public async Task<JObject> IsAuthorized(string token, IOIDConfigurationFactory configurationFactory)
        {
            JwtSecurityToken decodedToken = null;
            string authFailureReason = "";

            string authProvider = AuthConstants.AUTH_PROVIDER_MP;

            //Get the configuration manager for the mp provider
            ConfigurationManager<OpenIdConnectConfiguration> configurationManager;
            configurationManager = configurationFactory.GetConfiguration(authProvider);

            //try mp - if that doesn't work it must be okta
            try
            {
                decodedToken = await ValidateTokenAsync(token, configurationManager);
            }
            catch (SecurityTokenValidationException stvex)
            {
                //Check the specifics of the exception - if exception was a signature validation failure it may be an okta token
                // If so we will try to validate against okta config
                if (stvex.Message.Contains("IDX10501"))
                {
                    authProvider = AuthConstants.AUTH_PROVIDER_OKTA;
                    configurationManager = configurationFactory.GetConfiguration(authProvider);

                    //Otherwise try the okta token
                    try
                    {
                        decodedToken = await ValidateTokenAsync(token, configurationManager);
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

            JObject responseObject = new JObject();
            JObject authenticationObject = new JObject();
            JObject authorizationObject = new JObject();

            if (decodedToken != null)
            {
                authenticationObject["authenticated"] = true;
                authenticationObject["provider"] = authProvider;
                authenticationObject["message"] = authFailureReason;
                //tokenValidResponse["claims"] = new List<string>();
            }
            else
            {
                authenticationObject["authenticated"] = false;
                authenticationObject["provider"] = authProvider;
                authenticationObject["message"] = authFailureReason;
                //tokenValidResponse["claims"] = new List<string>();
            }

            responseObject["authentication"] = authenticationObject;
            responseObject["authorization"] = authorizationObject;

            return responseObject;
        }

        private async Task<JwtSecurityToken> ValidateTokenAsync(string token, ConfigurationManager<OpenIdConnectConfiguration> configurationManager)
        {
            var discoveryDocument = await configurationManager.GetConfigurationAsync();
            var signingKeys = discoveryDocument.SigningKeys;

            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = signingKeys,
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = false,
                //ValidAudience = "api://default",
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = discoveryDocument.Issuer
            };

            JwtSecurityToken decodedToken = JwtUtilities.ValidateAndDecode(token, validationParameters);

            return decodedToken;
        }
    }
}