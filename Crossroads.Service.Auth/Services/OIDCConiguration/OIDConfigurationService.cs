using System;
using System.Threading.Tasks;
using Crossroads.Microservice.Settings;
using Crossroads.Service.Auth.Interfaces;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Crossroads.Service.Auth.Services
{
    public class OIDConfigurationService : IOIDConfigurationService
    {
        private readonly string _oktaAuthServerUrl;
        private readonly string _mpAuthServerUrl;

        private ConfigurationManager<OpenIdConnectConfiguration> mpConfigurationManager;
        private ConfigurationManager<OpenIdConnectConfiguration> oktaConfigurationManager;

        private ISettingsService _settingsService;

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public OIDConfigurationService(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            _oktaAuthServerUrl = settingsService.GetSetting("OKTA_OAUTH_BASE_URL");
            _mpAuthServerUrl = settingsService.GetSetting("MP_OAUTH_BASE_URL");
        }

        public async Task<OpenIdConnectConfiguration> GetMpConfigAsync()
        {
            if (mpConfigurationManager == null)
            {
                mpConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                _mpAuthServerUrl + "/.well-known/openid-configuration",
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
                _oktaAuthServerUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            }

            return await oktaConfigurationManager.GetConfigurationAsync();
        }
    }
}
