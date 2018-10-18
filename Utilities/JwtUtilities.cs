using System.IdentityModel.Tokens.Jwt;
using System.Collections;
using System.Configuration;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;

namespace Crossroads.Service.Auth.Utilities
{
    public static class JwtUtilities
    {
        public static JwtSecurityToken ValidateAndDecode(string jwt, TokenValidationParameters validationParameters)
        {
            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

                return (JwtSecurityToken)rawValidatedToken;
                // Or, you can return the ClaimsPrincipal
                // (which has the JWT properties automatically mapped to .NET claims)
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failed validation!
                // TODO: Log it or display an error.
                throw new Exception($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                throw new Exception($"Token was invalid: {argex.Message}");
            }
        }
    }
    
}