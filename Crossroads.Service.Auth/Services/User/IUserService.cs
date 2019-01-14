using Crossroads.Web.Auth.Models;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IUserService
    {
        UserInfo GetUserInfo(string originalToken, CrossroadsDecodedToken crossroadsDecodedToken, string mpAPIToken);

        Authorization GetAuthorizations(CrossroadsDecodedToken crossroadsDecodedToken, string mpAPIToken, int mpContactId);
    }
}
