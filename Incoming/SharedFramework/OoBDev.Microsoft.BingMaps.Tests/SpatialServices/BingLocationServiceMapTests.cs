using BingMapsRESTToolkit;
using OoBDev.Microsoft.BingMaps.SpatialServices;
using OoBDev.SpatialServices.Contracts;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.Microsoft.BingMaps.Tests.SpatialServices
{
    [TestClass]
    public class BingLocationServiceMapTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private BingLocationServiceMap CreateBingLocationServiceMap() =>
            new BingLocationServiceMap();

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddressAsStringTest()
        {
            // Stage
            var data = new
            {
                Address = "Test Address",
                City = "Test City",
                State = "Test State",
                ZipCode = "Test ZipCode",
            };
            var expected = "Test Address, Test City, Test State, Test ZipCode";

            // Mock
            var address = mockRepository.Create<IAddress>();
            address.Setup(s => s.Address).Returns(data.Address);
            address.Setup(s => s.City).Returns(data.City);
            address.Setup(s => s.State).Returns(data.State);
            address.Setup(s => s.ZipCode).Returns(data.ZipCode);

            // Test
            var bingLocationServiceMap = this.CreateBingLocationServiceMap();

            var result = bingLocationServiceMap.AddressAsString(address.Object);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void LocationToPositionTest()
        {
            // Stage
            var location = new Location
            {
                Point = new Point
                {
                    Coordinates = new[]
                   {
                       1.234d,
                       2.345d,
                   }
                }
            };

            // Mock

            // Test
            var bingLocationServiceMap = this.CreateBingLocationServiceMap();


            var result = bingLocationServiceMap.LocationToPosition(location);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1.234m, result.Latitude);
            Assert.AreEqual(2.345m, result.Longitude);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void LocationToAddressTest()
        {
            // Stage
            var location = new Location
            {
                ConfidenceLevelType = ConfidenceLevelType.High,
                Address = new Address
                {
                    AddressLine = "address",
                    Locality = "city",
                    AdminDistrict = "state",
                    AdminDistrict2 = "county",
                    PostalCode = "zip",
                },
                Point = new Point
                {
                    Coordinates = new[]
                   {
                       1.234d,
                       2.345d,
                   }
                }
            };

            // Mock

            // Test
            var bingLocationServiceMap = this.CreateBingLocationServiceMap();
            var result = bingLocationServiceMap.LocationToAddress(location);
            this.TestContext.AddResult(result);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow(ConfidenceLevelType.High, ResultQuality.High)]
        [DataRow(ConfidenceLevelType.Low, ResultQuality.Low)]
        [DataRow(ConfidenceLevelType.Medium, ResultQuality.Medium)]
        [DataRow(ConfidenceLevelType.None, ResultQuality.Unknown)]
        [DataRow((ConfidenceLevelType)(-1), ResultQuality.Unknown)]
        [DataRow((ConfidenceLevelType)(int.MaxValue), ResultQuality.Unknown)]
        [DataRow((ConfidenceLevelType)(int.MinValue), ResultQuality.Unknown)]
        public void ConvertQualityTest(ConfidenceLevelType confidence, ResultQuality quality)
        {
            // Stage
            // Mock

            // Test
            var bingLocationServiceMap = this.CreateBingLocationServiceMap();

            var result = bingLocationServiceMap.ConvertQuality(confidence);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(quality, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
