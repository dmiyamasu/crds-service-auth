using System.IdentityModel.Tokens.Jwt;
using System.Collections;
using System.Configuration;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;

namespace Crossroads.Service.Auth.Utilities
{
    public static class JwtUtilities
    {
        public static async Task<JwtSecurityToken> ValidateTokenAsync(string token, ConfigurationManager<OpenIdConnectConfiguration> configurationManager)
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

        public static JwtSecurityToken ValidateAndDecode(
            string jwt,
            TokenValidationParameters validationParameters)
        {
            var claimsPrincipal = new JwtSecurityTokenHandler()
                .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

            return (JwtSecurityToken)rawValidatedToken;
            // Or, you can return the ClaimsPrincipal
            // (which has the JWT properties automatically mapped to .NET claims)
        }
    }
}