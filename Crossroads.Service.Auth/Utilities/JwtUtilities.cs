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