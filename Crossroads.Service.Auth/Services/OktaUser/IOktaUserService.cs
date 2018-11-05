using System;
using System.Collections.Generic;
using static Crossroads.Service.Auth.Services.JwtService;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IOktaUserService
    {
        int GetMpContactIdFromDecodedToken(CrossroadsDecodedToken decodedToken);

        IDictionary<int, string> GetRoles(CrossroadsDecodedToken decodedToken);
    }
}
