using OoBDev.Api.Census.Geocoding.SpatialServices.Models;
using OoBDev.Toolkit.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Api.Census.Geocoding.SpatialServices
{
    // https://geocoding.geo.census.gov/geocoder/Geocoding_Services_API.pdf
    // https://www.census.gov/programs-surveys/geography/technical-documentation/complete-technical-documentation/census-geocoder.html
    // https://www.census.gov/data/developers/guidance/api-user-guide.html
    public class LocationServiceClient : ILocationServiceClient
    {
        private readonly ICensusGeocodingConfig _configuration;
        private readonly IObjectConverter _converter;

        public LocationServiceClient(
            IObjectConverter converter,
            ICensusGeocodingConfig configuration
            )
        {
            _configuration = configuration;
            _converter = converter;
        }

        public async Task<IEnumerable<AddressMatchModel>> GetLocationsAsync(string address)
        {
            var url = string.Format(_configuration.UrlFormatter, address, _configuration.BenchmarkId, _configuration.VintageId);

            //TODO: look into ways of unit testing restful clients
            var httpClient = new HttpClient();
            var result = await httpClient.GetAsync(url);
            var content = await result.Content.ReadAsStringAsync();
            var response = await _converter.ConvertAsync<ResponseModel>(content);

            if (response.errors?.Any() ?? false)
            {
                throw new ApplicationException(string.Join("; ", response.errors));
            }

            return response?.result?.addressMatches ?? Enumerable.Empty<AddressMatchModel>();
        }

        public async Task<CoordinatesModel> GetPositionAsync(string address)
        {
            var results = await GetLocationsAsync(address).ConfigureAwait(false);
            var result = results.FirstOrDefault()?.coordinates;
            return result;
        }
    }
}