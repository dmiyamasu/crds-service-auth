using System.Collections.Generic;
using Crossroads.Service.Auth.Interfaces;
using System.Security.Claims;

namespace Crossroads.Service.Auth.Services
{
    public class OktaUserService : IOktaUserService
    {
        public int GetMpContactIdFromDecodedToken(CrossroadsDecodedToken decodedToken)
        {
            int mpContactId = -1;

            foreach (Claim claim in decodedToken.decodedToken.Claims)
            {
                if (claim.Type == "mpContactId")
                {
                    mpContactId = System.Convert.ToInt32(claim.Value);
                    break;
                }
            }

            return mpContactId;
        }

        public IDictionary<int, string> GetRoles(CrossroadsDecodedToken decodedToken)
        {
            return new Dictionary<int, string>();
        }
    }
}
