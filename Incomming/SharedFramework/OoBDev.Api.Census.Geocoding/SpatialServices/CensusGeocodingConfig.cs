using Microsoft.Extensions.Configuration;

namespace OoBDev.Api.Census.Geocoding.SpatialServices
{
    public class CensusGeocodingConfig : ICensusGeocodingConfig
    {
        public const int DefaultBenchmarkId = 4; //Current
        public const int DefaultVintageId = 4; // Current
        public const string DefaultUrlFormatter = "https://geocoding.geo.census.gov/geocoder/geographies/onelineaddress?address={0}&benchmark={1}&vintage={2}&format=json";

        private readonly IConfiguration _config;

        public CensusGeocodingConfig(
             IConfiguration config
            )
        {
            _config = config;
        }

        public int BenchmarkId => int.TryParse(_config[$"Census:Geocoding:{nameof(BenchmarkId)}"], out var benchmarkId) ? benchmarkId : DefaultBenchmarkId;
        public int VintageId => int.TryParse(_config[$"Census:Geocoding:{nameof(VintageId)}"], out var vintageId) ? vintageId : DefaultVintageId;

        public string UrlFormatter => _config[$"Census:Geocoding:{nameof(UrlFormatter)}"] switch
        {
            string value when !string.IsNullOrWhiteSpace(value) => value,
            _ => DefaultUrlFormatter
        };
    }
}