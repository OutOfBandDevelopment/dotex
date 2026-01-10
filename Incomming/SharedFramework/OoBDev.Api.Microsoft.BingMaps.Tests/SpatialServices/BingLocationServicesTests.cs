using OoBDev.TestUtilities;
using OoBDev.Api.Microsoft.BingMaps.SpatialServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using OoBDev.SpatialServices.Contracts;
using BingMapsRESTToolkit;
using System.Linq;
using OoBDev.Api.Microsoft.BingMaps.SpatialServices.Models;

namespace OoBDev.Api.Microsoft.BingMaps.Tests.SpatialServices
{
    [TestClass]
    public class BingLocationServicesTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IBingLocationServiceClient> mockClient;
        private Mock<IBingLocationServiceMap> mockMap;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockClient = this.mockRepository.Create<IBingLocationServiceClient>();
            this.mockMap = this.mockRepository.Create<IBingLocationServiceMap>();
        }

        private BingMapsLocationServices CreateBingLocationServices() =>
            new BingMapsLocationServices(this.mockClient.Object, this.mockMap.Object);

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task AddressToLatLongAsyncTest_IAddress()
        {
            // Stage
            string address = "test address";
            var locations = new[]
            {
                new Location()
                {
                    Name =address,
                },
            };

            // Mock
            var mockAddress = mockRepository.Create<IAddress>();
            var mockPosition = mockRepository.Create<IGlobalPosition>();
            mockMap.Setup(s => s.AddressAsString(mockAddress.Object)).Returns(address);
            mockClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(locations);
            mockMap.Setup(s => s.LocationToPosition(locations[0])).Returns(mockPosition.Object);

            // Test
            var bingLocationServices = this.CreateBingLocationServices();
            var result = await bingLocationServices.AddressToLatLongAsync(mockAddress.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockPosition.Object, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task AddressToLatLongAsyncTest_String()
        {
            // Stage
            string address = "test address";
            var locations = new[]
            {
                new Location()
                {
                    Name =address,
                },
            };

            // Mock
            var mockPosition = mockRepository.Create<IGlobalPosition>();
            mockClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(locations);
            mockMap.Setup(s => s.LocationToPosition(locations[0])).Returns(mockPosition.Object);

            // Test
            var bingLocationServices = this.CreateBingLocationServices();
            var result = await bingLocationServices.AddressToLatLongAsync(address);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockPosition.Object, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        //var locations = await _client.GetLocationsAsync(address).ConfigureAwait(false);
        //var position = _map.LocationToPosition(location);


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task LookupAddressTest_IAddress()
        {
            // Stage
            string address = "test address";
            var locations = new[]
            {
                new Location()
                {
                    Name = "not this",
                    ConfidenceLevelType = ConfidenceLevelType.None,
                },
                new Location()
                {
                    Name = address,
                    ConfidenceLevelType = ConfidenceLevelType.High,
                },
            };
            var resultAddress = new BingAddress(default, default, default, default, default, default, default);

            // Mock
            var mockAddress = mockRepository.Create<IAddress>();
            var mockPosition = mockRepository.Create<IGlobalPosition>();
            mockMap.Setup(s => s.AddressAsString(mockAddress.Object)).Returns(address);
            mockClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(locations);
            mockMap.Setup(s => s.LocationToAddress(It.IsAny<Location>())).Returns(resultAddress);

            // Test
            var bingLocationServices = this.CreateBingLocationServices();
            var result = await bingLocationServices.LookupAddress(mockAddress.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task LookupAddressTest_String()
        {
            // Stage
            string address = "test address";
            var locations = new[]
            {
                new Location()
                {
                    Name = "not this",
                    ConfidenceLevelType = ConfidenceLevelType.None,
                },
                new Location()
                {
                    Name = address,
                    ConfidenceLevelType = ConfidenceLevelType.High,
                },
            };
            var resultAddress = new BingAddress(default, default, default, default, default, default, default);

            // Mock
            var mockPosition = mockRepository.Create<IGlobalPosition>();
            mockClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(locations);
            mockMap.Setup(s => s.LocationToAddress(It.IsAny<Location>())).Returns(resultAddress);

            // Test
            var bingLocationServices = this.CreateBingLocationServices();
            var result = await bingLocationServices.LookupAddress(address);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());

            // Verify
            this.mockRepository.VerifyAll();
        }

        /*

        public Task<IEnumerable<IAddressResult>> LookupAddress(IAddress address) =>
            LookupAddress(_map.AddressAsString(address));

        public async Task<IEnumerable<IAddressResult>> LookupAddress(string address)
        {
            var locations = await _client.GetLocationsAsync(address).ConfigureAwait(false);
            var addresses = from location in locations
                            select _map.LocationToAddress(location);
            return addresses.ToArray();
        }
        */
    }
}
