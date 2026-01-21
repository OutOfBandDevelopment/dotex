using BingMapsRESTToolkit;
using OoBDev.Toolkit.Contracts.Net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.BingMaps.SpatialServices
{
    [ExcludeFromCodeCoverage]
    public class BingLocationRestClient : IBingLocationRestClient
    {
        private readonly string _apiKey;
        private readonly IWebProxyFactory<IBingLocationServiceClient> _proxyFactory;

        public BingLocationRestClient(
            IConfiguration config,
            IWebProxyFactory<IBingLocationServiceClient> proxyFactory
            )
        {
            _apiKey = config["Microsoft:BingMaps:ApiKey"];
            _proxyFactory = proxyFactory;
        }

        public async Task<IEnumerable<TResponse>> GetResourcesFromRequest<TResponse>(BaseRestRequest request)
            where TResponse : Resource
        {
            request.BingMapsKey = _apiKey;
            ServiceManager.Proxy = _proxyFactory.GetWebProxy();
            var result = await ServiceManager.GetResponseAsync(request).ConfigureAwait(false);
            if (result.StatusCode >= 400) throw new ApplicationException(string.Join(Environment.NewLine, result.ErrorDetails));
            return result?.ResourceSets?.FirstOrDefault()?.Resources.OfType<TResponse>() ?? Enumerable.Empty<TResponse>();
        }

    }
}
