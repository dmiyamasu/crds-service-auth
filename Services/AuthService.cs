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

namespace Crossroads.Service.Auth.Services
{
    public class AuthService : IAuthService
    {
        private OpenIdConnectConfiguration _oktaConfiguration;
        private OpenIdConnectConfiguration _mpConfiguration;

        public AuthService() 
        {
            Task loadSigningKeysTask = LoadSigningKeysAsync();
        }

        public async Task LoadSigningKeysAsync()
        {
            var oktaConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://dev-324490.oktapreview.com/oauth2/default/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            Task<OpenIdConnectConfiguration> oktaConfigTask = oktaConfigurationManager.GetConfigurationAsync();

            var mpConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                "https://adminint.crossroads.net/ministryplatformapi/oauth/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            Task<OpenIdConnectConfiguration> mpConfigTask = mpConfigurationManager.GetConfigurationAsync();

            _oktaConfiguration = await oktaConfigTask;
            _mpConfiguration = await mpConfigTask;
        }

        public JwtSecurityToken IsAuthorized(string token)
        {
            JwtSecurityToken decodedToken;

            //try okta - if that doesn't work it must be mp
            try 
            {
                decodedToken = ValidateMpToken(token);
            }
            catch (Exception)
            {
                decodedToken = ValidateOktaToken(token);
            }

            //decodedToken.
            return decodedToken;
        }

        private JwtSecurityToken ValidateOktaToken(string token)
        {
            var oktaValidationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = _oktaConfiguration.SigningKeys,
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = true,
                ValidAudience = "api://default",
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = _oktaConfiguration.Issuer
            };

            JwtSecurityToken decodedToken = JwtUtilities.ValidateAndDecode(token, oktaValidationParameters);

            return decodedToken;
        }

        private JwtSecurityToken ValidateMpToken(string token)
        {
            var mpValidationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = _mpConfiguration.SigningKeys,
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = false,
                //ValidAudience = "api://default",
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = true,
                ValidIssuer = _mpConfiguration.Issuer
            };

            JwtSecurityToken decodedToken = JwtUtilities.ValidateAndDecode(token, mpValidationParameters);

            return decodedToken;
        }
    }
}