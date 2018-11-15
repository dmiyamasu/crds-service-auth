using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IOIDConfigurationService
    {
        Task<OpenIdConnectConfiguration> GetMpConfigAsync();

        Task<OpenIdConnectConfiguration> GetOktaConfigAsync();
    }
}
