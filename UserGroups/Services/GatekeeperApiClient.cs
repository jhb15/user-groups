using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace UserGroups.Services
{
    public class GatekeeperApiClient : IGatekeeperApiClient
    {
        private readonly HttpClient client;
        private IConfigurationSection appConfig;
        private DiscoveryCache discoveryCache;
        private ILogger logger;

        public GatekeeperApiClient(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<GatekeeperApiClient> log)
        {
            appConfig = configuration.GetSection("UserGroups");
            discoveryCache = new DiscoveryCache(appConfig.GetValue<string>("GatekeeperUrl"));
            client = httpClientFactory.CreateClient("gatekeeper");
            logger = log;
        }

        private async Task<string> GetTokenAsync()
        {
            var discovery = await discoveryCache.GetAsync();
            if (discovery.IsError)
            {
                logger.LogError(discovery.Error);
                throw new GatekeeperApiException("Couldn't read discovery document.");
            }

            var tokenRequest = new ClientCredentialsTokenRequest
            {
                Address = discovery.TokenEndpoint,
                ClientId = appConfig.GetValue<string>("ClientId"),
                ClientSecret = appConfig.GetValue<string>("ClientSecret"),
                Scope = "gatekeeper"
            };
            var response = await client.RequestClientCredentialsTokenAsync(tokenRequest);
            if (response.IsError)
            {
                logger.LogError(response.Error);
                throw new GatekeeperApiException("Couldn't retrieve access token.");
            }
            return response.AccessToken;

        }

        public async Task<HttpResponseMessage> GetAsync(string path)
        {
            client.SetBearerToken(await GetTokenAsync());
            return await client.GetAsync(path);
        }

        public async Task<HttpResponseMessage> PostAsync(string path, object body)
        {
            client.SetBearerToken(await GetTokenAsync());
            return await client.PostAsJsonAsync(path, body);
        }
    }

    public class GatekeeperApiException : Exception
    {
        public GatekeeperApiException(string message) : base(message)
        {
        }
    }
}
