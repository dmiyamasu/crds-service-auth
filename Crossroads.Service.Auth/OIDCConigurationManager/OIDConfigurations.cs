using System;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Crossroads.Service.Auth.Configurations
{
    public class OIDConfigurations
    {
        private readonly string oktaAuthServerUrl = Environment.GetEnvironmentVariable("OKTA_OAUTH_BASE_URL");
        private readonly string mpAuthServerUrl = Environment.GetEnvironmentVariable("MP_OAUTH_BASE_URL");

        public ConfigurationManager<OpenIdConnectConfiguration> mpConfigurationManager{ get; private set; }
        public ConfigurationManager<OpenIdConnectConfiguration> oktaConfigurationManager{ get; private set; }

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public OIDConfigurations()
        {
            oktaConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                oktaAuthServerUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
            
            mpConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                mpAuthServerUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());
        }
    }
}
