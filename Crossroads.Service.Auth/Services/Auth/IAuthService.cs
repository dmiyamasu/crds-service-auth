using System.Threading.Tasks;
using Crossroads.Service.Auth.Models;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthDTO> Authorize(string token);
    }
}
