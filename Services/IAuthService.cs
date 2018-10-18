using System.IdentityModel.Tokens.Jwt;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IAuthService
    {
        JwtSecurityToken IsAuthorized(string token);
    }
}
