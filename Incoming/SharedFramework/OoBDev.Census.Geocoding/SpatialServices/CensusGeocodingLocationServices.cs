using OoBDev.SpatialServices.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Census.Geocoding.SpatialServices
{
    // https://geocoding.geo.census.gov/geocoder/Geocoding_Services_API.pdf
    // https://www.census.gov/programs-surveys/geography/technical-documentation/complete-technical-documentation/census-geocoder.html
    // https://www.census.gov/data/developers/guidance/api-user-guide.html
    public class CensusGeocodingLocationServices : ILocationServices
    {
        private readonly ILocationServiceClient _client;
        private readonly ILocationServiceMap _map;
        public CensusGeocodingLocationServices(
            ILocationServiceClient client,
            ILocationServiceMap map
            )
        {
            _client = client;
            _map = map;
        }

        public Task<IGlobalPosition> AddressToLatLongAsync(IAddress address) =>
            AddressToLatLongAsync(_map.AddressAsString(address));

        public async Task<IGlobalPosition> AddressToLatLongAsync(string address)
        {
            var postion = await _client.GetPositionAsync(address).ConfigureAwait(false);
            var mapped = _map.LocationToPosition(postion);
            return mapped;
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
