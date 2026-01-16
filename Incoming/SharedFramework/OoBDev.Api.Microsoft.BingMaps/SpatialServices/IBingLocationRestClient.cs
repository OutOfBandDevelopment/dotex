using BingMapsRESTToolkit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Api.Microsoft.BingMaps.SpatialServices
{
    public interface IBingLocationRestClient
    {
        Task<IEnumerable<TResponse>> GetResourcesFromRequest<TResponse>(BaseRestRequest request)
            where TResponse : Resource;
    }
}
