using GoogleApi;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Geocoding.Address.Request;
using GoogleApi.Entities.Maps.Geocoding.Common;
using GoogleApi.Entities.Maps.Geocoding.Common.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Google.Maps.SpatialServices
{
    public class LocationServiceClient : ILocationServiceClient
    {
        private readonly IConfiguration _configuration;
        public LocationServiceClient(
            IConfiguration configuration
            )
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<Result>> GetLocationsAsync(string address)
        {
#if NET6_0_OR_GREATER
            var api = GoogleMaps.Geocode.AddressGeocode;
#else
            var api = GoogleMaps.AddressGeocode;
#endif
            var response = await api.QueryAsync(new AddressGeocodeRequest
            {
                Key = _configuration["Google:Maps:ApiKey"],
                Address = address
            }).ConfigureAwait(false);

            if (response.Status != GoogleApi.Entities.Common.Enums.Status.Ok)
                throw new ApplicationException(response.Status.ToString());

            return response.Results;
        }

        public async Task<(Coordinate? location, GeometryLocationType? quality)> GetPositionAsync(string address)
        {
            var results = await GetLocationsAsync(address).ConfigureAwait(false);
            var result = results.FirstOrDefault();
            return (
                result?.Geometry?.Location,
                result?.Geometry?.LocationType
                );
        }
    }
}