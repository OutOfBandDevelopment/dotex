using OoBDev.TestUtilities;
using OoBDev.Api.Census.Geocoding.SpatialServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.SpatialServices.Contracts;
using OoBDev.Api.Census.Geocoding.SpatialServices.Models;

namespace OoBDev.Api.Census.Geocoding.Tests.SpatialServices
{
    [TestClass]
    public class LocationServiceMapTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private LocationServiceMap CreateLocationServiceMap() =>
            new LocationServiceMap();

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddressAsStringTest_Nulls()
        {
            // Stage

            // Mock
            var mockIAddress = mockRepository.Create<IAddress>();
            mockIAddress.Setup(s => s.Address).Returns("");
            mockIAddress.Setup(s => s.City).Returns("");
            mockIAddress.Setup(s => s.State).Returns("");
            mockIAddress.Setup(s => s.ZipCode).Returns("");

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.AddressAsString(mockIAddress.Object);

            // Assert
            Assert.AreEqual("", result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddressAsStringTest_Values()
        {
            // Stage

            // Mock
            var mockIAddress = mockRepository.Create<IAddress>();
            mockIAddress.Setup(s => s.Address).Returns("A");
            mockIAddress.Setup(s => s.City).Returns("C");
            mockIAddress.Setup(s => s.State).Returns("S");
            mockIAddress.Setup(s => s.ZipCode).Returns("Z");

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.AddressAsString(mockIAddress.Object);

            // Assert
            Assert.AreEqual("A, C, S, Z", result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void LocationToAddressTest_Values()
        {
            // Stage
            var location = new AddressMatchModel
            {
                addressComponents = new AddressComponentModel
                {
                    fromAddress = "from",
                    toAddress = "to",
                    streetName = "street",
                    city = "city",
                    state = "state",
                    zip = "zip",
                },
                geographies = new GeographiesModel
                {
                    Counties = new[]
                     {
                         new CountyModel
                         {
                             NAME = "county",
                         },
                     },
                },
                coordinates = new CoordinatesModel
                {
                    x = 123,
                    y = 456,
                },
            };

            // Mock

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.LocationToAddress(location);
            this.TestContext.AddResult(result);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void LocationToAddressTest_Null()
        {
            // Stage
            AddressMatchModel location = null;

            // Mock

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.LocationToAddress(location);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void LocationToPositionTest_Null()
        {
            // Stage
            CoordinatesModel location = null;

            // Mock

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.LocationToPosition(location);

            // Assert
            Assert.AreEqual(0, result.Latitude);
            Assert.AreEqual(0, result.Longitude);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void LocationToPositionTest_Values()
        {
            // Stage
            var location = new CoordinatesModel
            {
                x = 123.0,
                y = 456.0,
            };

            // Mock

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.LocationToPosition(location);

            // Assert
            Assert.AreEqual(location.y, (double)result.Latitude);
            Assert.AreEqual(location.x, (double)result.Longitude);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void ConvertQualityTest()
        {
            // Stage

            // Mock

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.ConvertQuality();

            // Assert
            Assert.AreEqual(ResultQuality.Medium, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
