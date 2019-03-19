using System.IdentityModel.Tokens.Jwt;
using System;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Threading.Tasks;
using Crossroads.Service.Auth.Interfaces;
using Crossroads.Service.Auth.Constants;
using Crossroads.Service.Auth.Exceptions;
using System.Security.Claims;

namespace Crossroads.Service.Auth.Services
{
    public class JwtService : IJwtService
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        struct JwtIssuer
        {
            public string authProvider;
            public OpenIdConnectConfiguration configuration;
        }

        public async Task<CrossroadsDecodedToken> DecodeAndValidateToken(string token, IOIDConfigurationService configService)
        {
            JwtSecurityToken decodedToken = DecodeToken(token);

            // Get updated configurations for auth servers
            var mpConfiguration = await configService.GetMpConfigAsync();
            var oktaConfiguration = await configService.GetOktaConfigAsync();

            JwtIssuer issuer = GetAndValidateIssuer(decodedToken, mpConfiguration, oktaConfiguration);

            ValidateToken(token, issuer.configuration);

            CrossroadsDecodedToken crossroadsDecodedToken = new CrossroadsDecodedToken
            {
                decodedToken = decodedToken,
                authProvider = issuer.authProvider
            };

            return crossroadsDecodedToken;
        }

        private JwtSecurityToken DecodeToken(string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            bool canValidateToken = tokenHandler.CanReadToken(token);

            if (!canValidateToken)
            {
                string errorMessage = "Unable to decode token, it was malformed, empty, or null";
                _logger.Info(errorMessage);
                throw new TokenMalformedException(errorMessage);
            }

            JwtSecurityToken decodedToken = tokenHandler.ReadJwtToken(token);
            return decodedToken;
        }

        private JwtIssuer GetAndValidateIssuer(JwtSecurityToken decodedToken, OpenIdConnectConfiguration mpConfiguration, OpenIdConnectConfiguration oktaConfiguration)
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
                string exceptionMessage = $"The token issuer: {decodedToken.Issuer} was invalid";
                _logger.Warn(exceptionMessage);
                throw new SecurityTokenInvalidIssuerException(exceptionMessage);
            }

            return issuer;
        }

        private void ValidateToken(string token, OpenIdConnectConfiguration configuration)
        {
            if (configuration.SigningKeys == null)
            {
                throw new ConfigurationSigningKeysIsNull();
            }

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