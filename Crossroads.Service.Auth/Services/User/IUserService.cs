using System;
using Crossroads.Service.Auth.Models;
using static Crossroads.Service.Auth.Services.JwtService;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IUserService
    {
        UserInfoDTO GetUserInfo(string originalToken, CrossroadsDecodedToken crossroadsDecodedToken, string mpAPIToken);

        AuthorizationDTO GetAuthorizations(CrossroadsDecodedToken crossroadsDecodedToken, string mpAPIToken, int mpContactId);
    }
}
