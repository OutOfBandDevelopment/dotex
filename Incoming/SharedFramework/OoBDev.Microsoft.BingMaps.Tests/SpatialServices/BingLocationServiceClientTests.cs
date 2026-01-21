using OoBDev.TestUtilities;
using OoBDev.Microsoft.BingMaps.SpatialServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using BingMapsRESTToolkit;
using System.Linq;

namespace OoBDev.Microsoft.BingMaps.Tests.SpatialServices
{
    [TestClass]
    public class BingLocationServiceClientTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IBingLocationRestClient> mockRest;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockRest = this.mockRepository.Create<IBingLocationRestClient>();
        }

        private BingLocationServiceClient CreateBingLocationServiceClient()
        {
            return new BingLocationServiceClient(
                this.mockRest.Object);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task GetLocationsAsyncTest()
        {
            // Stage
            var address = "test address";
            var locations = new[]
            {
                new Location()
                {
                    ConfidenceLevelType = ConfidenceLevelType.None,
                },
                new Location()
                {
                    ConfidenceLevelType = ConfidenceLevelType.High,
                },
            };

            // Mock
            mockRest.Setup(s => s.GetResourcesFromRequest<Location>(It.IsAny<GeocodeRequest>()))
                .ReturnsAsync(locations);

            // Test
            var bingLocationServiceClient = this.CreateBingLocationServiceClient();


            var result = await bingLocationServiceClient.GetLocationsAsync(address);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("High,None", string.Join(",", result.Select(i => i.ConfidenceLevelType.ToString())));

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
