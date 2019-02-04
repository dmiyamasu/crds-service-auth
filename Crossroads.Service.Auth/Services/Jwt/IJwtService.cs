using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Crossroads.Service.Auth.Interfaces
{
    public struct CrossroadsDecodedToken
    {
        public JwtSecurityToken decodedToken;
        public string authProvider;
    }

    public interface IJwtService
    {
        Task<CrossroadsDecodedToken> DecodeAndValidateToken(string token, IOIDConfigurationService configService);
        bool TokenIsOpenId(CrossroadsDecodedToken decodedToken);
    }
}
