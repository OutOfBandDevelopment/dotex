using OoBDev.TestUtilities;
using OoBDev.Google.Maps.SpatialServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.SpatialServices.Contracts;
using GoogleApi.Entities.Maps.Geocoding.Common.Enums;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.Geocoding.Common;
using GoogleApi.Entities.Common.Enums;

namespace OoBDev.Google.Maps.Tests.SpatialServices
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

        private LocationServiceMap CreateLocationServiceMap() => new LocationServiceMap();

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
        public void LocationToPositionTest()
        {
            // Stage
            var location = new Coordinate(123.4, 567.8);
            var confidence = GeometryLocationType.Range_Interpolated;

            // Mock

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.LocationToPosition(location, confidence);

            // Assert
            Assert.AreEqual((decimal)location.Latitude, result.Latitude);
            Assert.AreEqual((decimal)location.Longitude, result.Longitude);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow(null, ResultQuality.Unknown)]
        [DataRow(GeometryLocationType.Rooftop, ResultQuality.High)]
        [DataRow(GeometryLocationType.Geometric_Center, ResultQuality.Medium)]
        [DataRow(GeometryLocationType.Range_Interpolated, ResultQuality.Medium)]
        [DataRow(GeometryLocationType.Approximate, ResultQuality.Low)]
        public void ConvertQualityTest(GeometryLocationType? input, ResultQuality expected)
        {
            // Stage

            // Mock

            // Test
            var locationServiceMap = this.CreateLocationServiceMap();
            var result = locationServiceMap.ConvertQuality(input);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void LocationToAddressTest()
        {
            // Stage

            var location = new Result
            {
                AddressComponents = new []
                {
                     new AddressComponent { Types = new [] { AddressComponentType.Street_Number }, LongName = "123" },
                     new AddressComponent { Types = new [] { AddressComponentType.Route }, LongName = "Street" },
                     new AddressComponent { Types = new [] { AddressComponentType.Locality }, LongName = "City" },
                     new AddressComponent { Types = new [] { AddressComponentType.Administrative_Area_Level_1 }, LongName = "State" },
                     new AddressComponent { Types = new [] { AddressComponentType.Postal_Code }, LongName = "Zip" },
                     new AddressComponent { Types = new [] { AddressComponentType.Administrative_Area_Level_2 }, LongName = "County" },
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
    }
}
