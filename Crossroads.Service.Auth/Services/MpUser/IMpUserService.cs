using System.Collections.Generic;
using Crossroads.Web.Auth.Models;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IMpUserService
    {
        int GetMpContactIdFromToken(string token);

        MpUserInfo GetMpUserInfoFromContactId(int contactId, string mpAPIToken);

        Dictionary<int, string> GetRoles(string mpAPIToken, int mpContactId);
    }
}
