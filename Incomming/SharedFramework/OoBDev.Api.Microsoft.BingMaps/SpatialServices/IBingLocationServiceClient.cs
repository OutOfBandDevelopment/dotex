using BingMapsRESTToolkit;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Api.Microsoft.BingMaps.SpatialServices
{
    public interface IBingLocationServiceClient
    {
        Task<IEnumerable<Location>> GetLocationsAsync(string address);
    }
}
