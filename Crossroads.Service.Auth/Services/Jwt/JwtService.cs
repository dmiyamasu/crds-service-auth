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

        public bool TokenIsOpenId(CrossroadsDecodedToken decodedToken)
        {
            bool isOpenId = false;
            foreach (Claim claim in decodedToken.decodedToken.Claims)
            {
                if (claim.Type == "scope")
                {
                    if (claim.Value == "openid")
                    {
                        isOpenId = true;
                        break;
                    }
                }
            }

            return isOpenId;
        }

        public async Task<CrossroadsDecodedToken> DecodeAndValidateToken(string token, IOIDConfigurationService configService)
        {
            JwtSecurityToken decodedToken = DecodeToken(token);

            // Get updated configurations for auth servers
            var mpConfiguration = await configService.GetMpConfigAsync();
            var oktaConfiguration = await configService.GetOktaConfigAsync();

            JwtIssuer issuer = GetAndValidateIssuer(decodedToken, mpConfiguration, oktaConfiguration);

            ValidateToken(token, issuer);

            CrossroadsDecodedToken crossroadsDecodedToken = new CrossroadsDecodedToken
            {
                decodedToken = decodedToken,
                authProvider = issuer.authProvider
            };

            return crossroadsDecodedToken;
        }

        private JwtSecurityToken DecodeToken(string token)
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
                string exceptionMessage = "The token issuer: " + decodedToken.Issuer + " was invalid";
                _logger.Debug(exceptionMessage);
                throw new SecurityTokenInvalidIssuerException(exceptionMessage);
            }

            return issuer;
        }

        private void ValidateToken(string token, JwtIssuer issuer)
        {
            var validationParameters = GetValidationParameters(issuer);

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            SecurityToken decodedToken;
            tokenHandler.ValidateToken(token, validationParameters, out decodedToken);
        }

        private TokenValidationParameters GetValidationParameters (JwtIssuer issuer)
        {
            bool validateLifetime = true;

            // If the token was from mp we have some funky logic that refreshes the token elsewhere
            if (issuer.authProvider == AuthConstants.AUTH_PROVIDER_MP)
            {
                validateLifetime = false;
            }

            return new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = issuer.configuration.SigningKeys,
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = validateLifetime,
                ValidateLifetime = validateLifetime,
                // Ensure the token audience matches our audience value (default true):
                ValidateAudience = false,
                // Ensure the token was issued by a trusted authorization server (default true):
                ValidateIssuer = false
            };
        }
    }
}