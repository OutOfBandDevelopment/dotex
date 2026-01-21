using BingMapsRESTToolkit;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.BingMaps.SpatialServices
{
    [ExcludeFromCodeCoverage]
    public class BingLocationServiceClient : IBingLocationServiceClient
    {
        private readonly IBingLocationRestClient _rest;

        public BingLocationServiceClient(
            IBingLocationRestClient rest
            )
        {
            _rest = rest;
        }

        public async Task<IEnumerable<Location>> GetLocationsAsync(string address)
        {
            var request = new GeocodeRequest() { Query = address, };
            var locations = await _rest.GetResourcesFromRequest<Location>(request).ConfigureAwait(false);
            var query = from location in locations
                        orderby location.ConfidenceLevelType == ConfidenceLevelType.None ? int.MaxValue : (int)location.ConfidenceLevelType
                        select location;
            return query.ToArray();
        }
    }
}
