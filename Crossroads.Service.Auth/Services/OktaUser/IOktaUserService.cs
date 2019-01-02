using System.Collections.Generic;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IOktaUserService
    {
        int GetMpContactIdFromDecodedToken(CrossroadsDecodedToken decodedToken);

        IDictionary<int, string> GetRoles(CrossroadsDecodedToken decodedToken);
    }
}
