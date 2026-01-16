using OoBDev.Api.Microsoft.BingMaps.SpatialServices;
using OoBDev.Api.Microsoft.BingMaps.SpatialServices.Models;
using OoBDev.SpatialServices.Contracts;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Api.Microsoft.BingMaps.Tests.SpatialServices
{
    [TestClass]
    public class BingGlobalPositionTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateBingGlobalPositionTest_Double()
        {
            // Stage
            var data = new
            {
                lat = 1.234d,
                @long = 2.345d,
                quality = ResultQuality.High,
            };
            // Mock

            // Test
            var result = new BingGlobalPosition(data.lat, data.@long, data.quality);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual((decimal)data.lat, result.Latitude);
            Assert.AreEqual((decimal)data.@long, result.Longitude);
            Assert.AreEqual(data.quality, result.Quality);

            // Verify
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateBingGlobalPositionTest_Decimal()
        {
            // Stage
            var data = new
            {
                lat = 1.234m,
                @long = 2.345m,
                quality = ResultQuality.High,
            };
            // Mock

            // Test
            var result = new BingGlobalPosition(data.lat, data.@long, data.quality);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(data.lat, result.Latitude);
            Assert.AreEqual(data.@long, result.Longitude);
            Assert.AreEqual(data.quality, result.Quality);

            // Verify
        }
    }
}
