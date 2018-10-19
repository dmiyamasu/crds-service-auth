using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Crossroads.Service.Auth.Services;
using System.Linq;
using Crossroads.Service.Auth.Interfaces;

namespace Crossroads.Service.Auth.Factories
{
    public class OIDConfigurationFactory : IOIDConfigurationFactory
    {
        private readonly string oktaAuthServerUrl = Environment.GetEnvironmentVariable("OKTA_OAUTH_BASE_URL");
        private readonly string mpAuthServerUrl = Environment.GetEnvironmentVariable("MP_OAUTH_BASE_URL");

        private readonly IDictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _configurations;

        public OIDConfigurationFactory()
        {
            var oktaConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                oktaAuthServerUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

            var mpConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                mpAuthServerUrl + "/.well-known/openid-configuration",
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever());

            _configurations = new Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();
            _configurations.Add(Constants.Constants.AUTH_PROVIDER_OKTA, oktaConfigurationManager);
            _configurations.Add(Constants.Constants.AUTH_PROVIDER_MP, mpConfigurationManager);
        }

        public ConfigurationManager<OpenIdConnectConfiguration> GetConfiguration(string name)
        {
            if (_configurations.TryGetValue(name, out var client))
                return client;

            // handle error
            throw new ArgumentException(nameof(name));
        }
    }
}
