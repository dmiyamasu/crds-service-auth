using System.Collections.Generic;
using Crossroads.Service.Auth.Models;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IMpUserService
    {
        int GetMpContactIdFromToken(string token);

        MpUserInfoDTO GetMpUserInfoFromContactId(int contactId, string mpAPIToken);

        Dictionary<int, string> GetRoles(string mpAPIToken, int mpContactId);
    }
}
