using OoBDev.SpatialServices.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OoBDev.Api.Microsoft.BingMaps.SpatialServices.Models;

namespace OoBDev.Api.Microsoft.BingMaps.SpatialServices
{
    public class BingMapsLocationServices : ILocationServices
    {
        private readonly IBingLocationServiceClient _client;
        private readonly IBingLocationServiceMap _map;
        public BingMapsLocationServices(
            IBingLocationServiceClient client,
            IBingLocationServiceMap map
            )
        {
            _client = client;
            _map = map;
        }

        public Task<IGlobalPosition> AddressToLatLongAsync(IAddress address) =>
            AddressToLatLongAsync(_map.AddressAsString(address));

        public async Task<IGlobalPosition> AddressToLatLongAsync(string address)
        {
            var locations = await _client.GetLocationsAsync(address).ConfigureAwait(false);
            var location = locations.FirstOrDefault();
            var position = _map.LocationToPosition(location);
            return position ??  BingGlobalPosition.Unknown;
        }

        public Task<IEnumerable<IAddressResult>> LookupAddress(IAddress address) =>
            LookupAddress(_map.AddressAsString(address));

        public async Task<IEnumerable<IAddressResult>> LookupAddress(string address)
        {
            var locations = await _client.GetLocationsAsync(address).ConfigureAwait(false);
            var addresses = from location in locations
                            select _map.LocationToAddress(location);
            return addresses.ToArray();
        }
    }
}
