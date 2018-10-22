using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<JObject> IsAuthorized(string token, IOIDConfigurationFactory configurationFactory);
    }
}
