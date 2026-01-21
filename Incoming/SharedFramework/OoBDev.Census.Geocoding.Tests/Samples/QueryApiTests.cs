using OoBDev.Census.Geocoding.SpatialServices;
using OoBDev.Census.Geocoding.SpatialServices.Models;
using OoBDev.TestUtilities;
using OoBDev.Toolkit;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Census.Geocoding.Tests.Samples
{
    [TestClass]
    [TestCategory(TestCategories.DevLocal)]
    public class QueryApiTests
    {
        public TestContext TestContext { get; set; }

        /// <summary>
        ///  Geocode Test
        /// </summary>
        [DataTestMethod]
        [DataRow("Seattle")]
        [DataRow("Columbus, Ohio")]
        [DataRow("576 S. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("560 N. Grant Ave, Columbus, Ohio 43206")]
        [TestCategory(TestCategories.DevLocal)]
        public async Task GeoCodeTest(string query)
        {
            var httpClient = new HttpClient();

            // https://geocoding.geo.census.gov/geocoder/benchmarks?format=json
            // https://geocoding.geo.census.gov/geocoder/vintages?benchmark=4&format=json
            var benchmarkId = 4; //current
            var vintageId = 4; //current

            // https://geocoding.geo.census.gov/geocoder/geographies/onelineaddress?address=576+s+grant+ave+columbus+ohio&benchmark=4&vintage=4&format=json
            // var url = $"https://geocoding.geo.census.gov/geocoder/locations/onelineaddress?address={query}&benchmark={4}&format=json";
            var url = $"https://geocoding.geo.census.gov/geocoder/geographies/onelineaddress?address={query}&benchmark={benchmarkId}&vintage={vintageId}&format=json";

            var result = await httpClient.GetAsync(url);
            var content = await result.Content.ReadAsStringAsync();

            var sp = new ServiceCollection()
                .AddToolkitServices()
                .BuildServiceProvider();

            var converter = sp.GetRequiredService<IObjectConverter>();
            var response = await converter.ConvertAsync<ResponseModel>(content);

            TestContext.WriteLine(content);
            TestContext.WriteLine($"{response}");
        }

        /// <summary>
        ///  Geocode Test
        /// </summary>
        [DataTestMethod]
        [DataRow("Seattle")]
        [DataRow("Columbus, Ohio")]
        [DataRow("576 S. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("560 N. Grant Ave, Columbus, Ohio 43206")]
        [TestCategory(TestCategories.DevLocal)]
        public async Task GetPositionAsyncTest(string query)
        {
            var client = GetClient();
            var result = await client.GetPositionAsync(query);

            TestContext.WriteLine(string.Join(";", result?.y, result?.x));
        }

        /// <summary>
        ///  Geocode Test
        /// </summary>
        [DataTestMethod]
        [DataRow("Seattle")]
        [DataRow("Columbus, Ohio")]
        [DataRow("576 S. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("560 N. Grant Ave, Columbus, Ohio 43206")]
        [TestCategory(TestCategories.DevLocal)]
        public async Task GetLocationsAsyncTest(string query)
        {
            var client = GetClient();
            var result = await client.GetLocationsAsync(query);

            foreach (var item in result)
                TestContext.WriteLine(item.ToString());
        }


        /// <summary>
        ///  Geocode Test
        /// </summary>
        [DataTestMethod]
        [DataRow("Seattle")]
        [DataRow("Columbus, Ohio")]
        [DataRow("576 S. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("560 N. Grant Ave, Columbus, Ohio 43206")]
        [TestCategory(TestCategories.DevLocal)]
        public async Task GetLocationsAsyncTest_Stacked(string query)
        {
            var client = GetClient();
            var map = new LocationServiceMap();
            var service = new CensusGeocodingLocationServices(
                client,
                map
                );
            var result = await service.LookupAddress(query);
            foreach (var item in result)
                TestContext.WriteLine(item.ToString());
        }

        /// <summary>
        ///  Geocode Test
        /// </summary>
        [DataTestMethod]
        [DataRow("Seattle")]
        [DataRow("Columbus, Ohio")]
        [DataRow("576 S. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("560 N. Grant Ave, Columbus, Ohio 43206")]
        [TestCategory(TestCategories.DevLocal)]
        public async Task GeoCodeTest_Stacked(string query)
        {
            var client = GetClient();
            var map = new LocationServiceMap();
            var service = new CensusGeocodingLocationServices(
                client,
                map
                );

            var result = await service.AddressToLatLongAsync(query);

            TestContext.WriteLine(string.Join(";", result.Latitude, result.Longitude, result.Quality));
        }

        private ILocationServiceClient GetClient() =>
            new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddDebugTestServices(this.TestContext)
                .AddCensusGeocodingServices()
                .AddToolkitServices()
                .BuildServiceProvider()
                .GetRequiredService<ILocationServiceClient>()
            ;
    }
}