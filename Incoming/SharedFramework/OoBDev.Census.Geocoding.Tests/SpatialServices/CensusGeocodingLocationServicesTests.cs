using OoBDev.TestUtilities;
using OoBDev.Census.Geocoding.SpatialServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using OoBDev.SpatialServices.Contracts;
using OoBDev.Census.Geocoding.SpatialServices.Models;
using System.Linq;

namespace OoBDev.Census.Geocoding.Tests.SpatialServices
{
    [TestClass]
    public class CensusGeocodingLocationServicesTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<ILocationServiceClient> mockLocationServiceClient;
        private Mock<ILocationServiceMap> mockLocationServiceMap;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLocationServiceClient = this.mockRepository.Create<ILocationServiceClient>();
            this.mockLocationServiceMap = this.mockRepository.Create<ILocationServiceMap>();
        }

        private CensusGeocodingLocationServices CreateCensusGeocodingLocationServices() =>
            new CensusGeocodingLocationServices(
                this.mockLocationServiceClient.Object,
                this.mockLocationServiceMap.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task AddressToLatLongAsyncTest_Address()
        {
            // Stage
            string address = "test address";
            var position = new CoordinatesModel();

            // Mock
            var mockAddress = mockRepository.Create<IAddress>();
            var mockGlobalPosition = mockRepository.Create<IGlobalPosition>();

            mockLocationServiceMap.Setup(s => s.AddressAsString(mockAddress.Object)).Returns(address);
            mockLocationServiceClient.Setup(s => s.GetPositionAsync(address)).ReturnsAsync(position);
            mockLocationServiceMap.Setup(s => s.LocationToPosition(position)).Returns(mockGlobalPosition.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateCensusGeocodingLocationServices();
            var result = await censusGeocodingLocationServices.AddressToLatLongAsync(mockAddress.Object);

            // Assert
            Assert.AreEqual(mockGlobalPosition.Object, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task AddressToLatLongAsyncTest_String()
        {
            // Stage
            string address = "test address";
            var position = new CoordinatesModel();

            // Mock
            var mockGlobalPosition = mockRepository.Create<IGlobalPosition>();

            mockLocationServiceClient.Setup(s => s.GetPositionAsync(address)).ReturnsAsync(position);
            mockLocationServiceMap.Setup(s => s.LocationToPosition(position)).Returns(mockGlobalPosition.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateCensusGeocodingLocationServices();
            var result = await censusGeocodingLocationServices.AddressToLatLongAsync(address);

            // Assert
            Assert.AreEqual(mockGlobalPosition.Object, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task LookupAddressTest_Address()
        {
            // Stage
            string address = "test address";
            var addresses = new[] { new AddressMatchModel() };

            // Mock
            var mockAddress = mockRepository.Create<IAddress>();
            var mockAddressResult = mockRepository.Create<IAddressResult>();

            mockLocationServiceMap.Setup(s => s.AddressAsString(mockAddress.Object)).Returns(address);
            mockLocationServiceClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(addresses);
            mockLocationServiceMap.Setup(s => s.LocationToAddress(addresses.Single())).Returns(mockAddressResult.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateCensusGeocodingLocationServices();
            var result = await censusGeocodingLocationServices.LookupAddress(mockAddress.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task LookupAddressTest_String()
        {
            // Stage
            string address = "test address";
            var addresses = new[] { new AddressMatchModel() };

            // Mock
            var mockAddressResult = mockRepository.Create<IAddressResult>();

            mockLocationServiceClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(addresses);
            mockLocationServiceMap.Setup(s => s.LocationToAddress(addresses.Single())).Returns(mockAddressResult.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateCensusGeocodingLocationServices();
            var result = await censusGeocodingLocationServices.LookupAddress(address);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
