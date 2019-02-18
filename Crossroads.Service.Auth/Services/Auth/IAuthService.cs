using System.Threading.Tasks;
using Crossroads.Web.Auth.Models;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthDTO> GetAuthorization(string token, string impersonateUserName);
    }
}
