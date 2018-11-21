using System;
using System.Threading.Tasks;
using Crossroads.Service.Auth.Interfaces;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Crossroads.Service.Auth.Services
{
    public class OIDConfigurationService : IOIDConfigurationService
    {
        private readonly string oktaAuthServerUrl = Environment.GetEnvironmentVariable("OKTA_OAUTH_BASE_URL");
        private readonly string mpAuthServerUrl = Environment.GetEnvironmentVariable("MP_OAUTH_BASE_URL");

        private ConfigurationManager<OpenIdConnectConfiguration> mpConfigurationManager;
        private ConfigurationManager<OpenIdConnectConfiguration> oktaConfigurationManager;

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public async Task<OpenIdConnectConfiguration> GetMpConfigAsync()
        {
            if (mpConfigurationManager == null)
            {
                mpConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                mpAuthServerUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            }

            return await mpConfigurationManager.GetConfigurationAsync();
        }

        public async Task<OpenIdConnectConfiguration> GetOktaConfigAsync()
        {
            if (oktaConfigurationManager == null)
            {
                oktaConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                oktaAuthServerUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            }

            return await oktaConfigurationManager.GetConfigurationAsync();
        }
    }
}
