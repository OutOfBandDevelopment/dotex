using OoBDev.Google.Maps.SpatialServices;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.Google.Maps.Tests.Samples
{
    //https://raw.githubusercontent.com/microsoft/BingMapsRESTToolkit/master/Samples/Console/RESTToolkitTestConsoleApp/Program.cs
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
            var config = this.TestContext.GetService<IConfiguration>();
            var httpClient = new HttpClient();

            var apiKey = config["Google:Maps:ApiKey"];

            //https://maps.googleapis.com/maps/api/geocode/json?address=1600+Amphitheatre+Parkway,+Mountain+View,+CA&key=YOUR_API_KEY

            var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={query?.Replace(" ", "+")}&key={apiKey}";

            var result = await httpClient.GetAsync(url);
            var content = await result.Content.ReadAsStringAsync();

            TestContext.WriteLine(content);
        }

        /// <summary>
        ///  Geocode Test
        /// </summary>
        [DataTestMethod]
        [DataRow("Seattle")]
        [DataRow("Columbus, Ohio")]
        [DataRow("576 S. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("560 N. Grant Ave, Columbus, Ohio 43206")]
        [DataRow("405 Robinson St Franklin GA 30217")]        
        [TestCategory(TestCategories.DevLocal)]
        public async Task GetPositionAsyncTest(string query)
        {
            var config = this.TestContext.GetService<IConfiguration>();
            var httpClient = new LocationServiceClient(config);

            var (location, quality) = await httpClient.GetPositionAsync(query);

            TestContext.WriteLine(string.Join(";", location, quality));
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
            var config = this.TestContext.GetService<IConfiguration>();
            var httpClient = new LocationServiceClient(config);

            var result = await httpClient.GetLocationsAsync(query);

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
            var config = this.TestContext.GetService<IConfiguration>();
            var client = new LocationServiceClient(config);
            var map = new LocationServiceMap();
            var service = new GoogleMapsLocationServices(
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
            var config = this.TestContext.GetService<IConfiguration>();
            var client = new LocationServiceClient(config);
            var map = new LocationServiceMap();
            var service = new GoogleMapsLocationServices(
                client,
                map
                );

            var result = await service.AddressToLatLongAsync(query);

            TestContext.WriteLine(string.Join(";", result.Latitude, result.Longitude, result.Quality));
        }
    }
}