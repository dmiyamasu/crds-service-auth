using System;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Crossroads.Service.Auth.Interfaces
{
    public interface IOIDConfigurationFactory
    {
        ConfigurationManager<OpenIdConnectConfiguration> GetConfiguration(string name);
    }
}
