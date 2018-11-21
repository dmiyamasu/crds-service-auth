using System.Collections.Generic;
using Crossroads.Service.Auth.Interfaces;
using static Crossroads.Service.Auth.Services.JwtService;

namespace Crossroads.Service.Auth.Services
{
    public class OktaUserService : IOktaUserService
    {
        public OktaUserService()
        {
            
        }

        public int GetMpContactIdFromDecodedToken(CrossroadsDecodedToken decodedToken)
        {
            //TODO: Somehow read the contactId value
            return -1;
        }

        public IDictionary<int, string> GetRoles(CrossroadsDecodedToken decodedToken)
        {
            return new Dictionary<int, string>();
        }
    }
}
