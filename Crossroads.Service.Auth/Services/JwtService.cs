using System.IdentityModel.Tokens.Jwt;
using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;
using Crossroads.Service.Auth.Factories;
using Crossroads.Service.Auth.Constants;
using Crossroads.Service.Auth.Exceptions;

namespace Crossroads.Service.Auth.Services
{
    public static class JwtService
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public struct CrossroadsDecodedToken
        {
            public JwtSecurityToken decodedToken;
            public string authProvider;
        }

        struct JwtIssuer
        {
            public string authProvider;
            public OpenIdConnectConfiguration configuration;
        }

        public static async Task<CrossroadsDecodedToken> DecodeAndValidateToken(string token, OIDConfigurationFactory configurationFactory)
        {
            JwtSecurityToken decodedToken = DecodeToken(token);

            // Get updated configurations for auth servers
            var mpConfiguration = await configurationFactory.mpConfigurationManager.GetConfigurationAsync();
            var oktaConfiguration = await configurationFactory.oktaConfigurationManager.GetConfigurationAsync();

            JwtIssuer issuer = GetAndValidateIssuer(decodedToken, mpConfiguration, oktaConfiguration);

            ValidateToken(token, issuer.configuration);

            CrossroadsDecodedToken crossroadsDecodedToken = new CrossroadsDecodedToken
            {
                decodedToken = decodedToken,
                authProvider = issuer.authProvider
            };

            return crossroadsDecodedToken;
        }

        private static JwtSecurityToken DecodeToken(string token)
        {
            try
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken decodedToken = tokenHandler.ReadJwtToken(token);
                return decodedToken;
            }
            catch (ArgumentException ex)
            {
                _logger.Debug(ex.Message);
                throw new TokenMalformedException(ex.Message);
            }
        }

        private static JwtIssuer GetAndValidateIssuer(JwtSecurityToken decodedToken, OpenIdConnectConfiguration mpConfiguration, OpenIdConnectConfiguration oktaConfiguration)
        {
            JwtIssuer issuer = new JwtIssuer();

            if (decodedToken.Issuer == mpConfiguration.Issuer)
            {
                issuer.authProvider = AuthConstants.AUTH_PROVIDER_MP;
                issuer.configuration = mpConfiguration;
            }
            else if (decodedToken.Issuer == oktaConfiguration.Issuer)
            {
                issuer.authProvider = AuthConstants.AUTH_PROVIDER_OKTA;
                issuer.configuration = oktaConfiguration;
            }
            else
            {
                string exceptionMessage = "The token issuer: " + decodedToken.Issuer + " was invalid";
                _logger.Debug(exceptionMessage);
                throw new SecurityTokenInvalidIssuerException(exceptionMessage);
            }

            return issuer;
        }

        private static void ValidateToken(string token, OpenIdConnectConfiguration configuration)
        {
            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = configuration.SigningKeys,
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = false,
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = false
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken decodedToken;
            tokenHandler.ValidateToken(token, validationParameters, out decodedToken);
        }
    }
}