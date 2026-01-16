using OoBDev.TestUtilities;
using OoBDev.Api.Google.Maps.SpatialServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using OoBDev.SpatialServices.Contracts;
using GoogleApi.Entities.Maps.Geocoding.Common;
using System.Linq;
using GoogleApi.Entities.Maps.Geocoding.Common.Enums;
using GoogleApi.Entities.Common;

namespace OoBDev.Api.Google.Maps.Tests.SpatialServices
{
    [TestClass]
    public class GoogleMapsLocationServicesTests
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

        private GoogleMapsLocationServices CreateGoogleMapsLocationServices() =>
            new GoogleMapsLocationServices(
                this.mockLocationServiceClient.Object,
                this.mockLocationServiceMap.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task AddressToLatLongAsyncTest_Address()
        {
            // Stage
            string address = "test address";
            var position = (location: new Coordinate (0,0), quality: GeometryLocationType.Geometric_Center);

            // Mock
            var mockAddress = mockRepository.Create<IAddress>();
            var mockGlobalPosition = mockRepository.Create<IGlobalPosition>();

            mockLocationServiceMap.Setup(s => s.AddressAsString(mockAddress.Object)).Returns(address);
            mockLocationServiceClient.Setup(s => s.GetPositionAsync(address)).ReturnsAsync(position);
            mockLocationServiceMap.Setup(s => s.LocationToPosition(position.location, position.quality)).Returns(mockGlobalPosition.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateGoogleMapsLocationServices();
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
            var position = (location: new Coordinate(0, 0), quality: GeometryLocationType.Geometric_Center);

            // Mock
            var mockGlobalPosition = mockRepository.Create<IGlobalPosition>();

            mockLocationServiceClient.Setup(s => s.GetPositionAsync(address)).ReturnsAsync(position);
            mockLocationServiceMap.Setup(s => s.LocationToPosition(position.location, position.quality)).Returns(mockGlobalPosition.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateGoogleMapsLocationServices();
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
            var addresses = new[] { new Result() };

            // Mock
            var mockAddress = mockRepository.Create<IAddress>();
            var mockAddressResult = mockRepository.Create<IAddressResult>();

            mockLocationServiceMap.Setup(s => s.AddressAsString(mockAddress.Object)).Returns(address);
            mockLocationServiceClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(addresses);
            mockLocationServiceMap.Setup(s => s.LocationToAddress(addresses.Single())).Returns(mockAddressResult.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateGoogleMapsLocationServices();
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
            var addresses = new[] { new Result() };

            // Mock
            var mockAddressResult = mockRepository.Create<IAddressResult>();

            mockLocationServiceClient.Setup(s => s.GetLocationsAsync(address)).ReturnsAsync(addresses);
            mockLocationServiceMap.Setup(s => s.LocationToAddress(addresses.Single())).Returns(mockAddressResult.Object);

            // Test
            var censusGeocodingLocationServices = this.CreateGoogleMapsLocationServices();
            var result = await censusGeocodingLocationServices.LookupAddress(address);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
