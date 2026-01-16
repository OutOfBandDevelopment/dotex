using OoBDev.TestUtilities;
using OoBDev.Api.Census.Geocoding.SpatialServices;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.Api.Census.Geocoding.Tests.SpatialServices
{
    [TestClass]
    public class CensusGeocodingConfigTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private CensusGeocodingConfig CreateCensusGeocodingConfig() =>
            new CensusGeocodingConfig(
                this.mockConfiguration.Object
                );

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow(null, CensusGeocodingConfig.DefaultBenchmarkId)]
        [DataRow("bad value", CensusGeocodingConfig.DefaultBenchmarkId)]
        [DataRow("2", 2)]
        public void BenchmarkIdTest(string value, int expected)
        {
            // Stage

            // Mock
            mockConfiguration.Setup(s => s[$"Census:Geocoding:{nameof(CensusGeocodingConfig.BenchmarkId)}"]).Returns(value);

            // Test
            var censusGeocodingConfig = this.CreateCensusGeocodingConfig();
            var result = censusGeocodingConfig.BenchmarkId;

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow(null, CensusGeocodingConfig.DefaultVintageId)]
        [DataRow("bad value", CensusGeocodingConfig.DefaultVintageId)]
        [DataRow("2", 2)]
        public void VintageIdTest(string value, int expected)
        {
            // Stage

            // Mock
            mockConfiguration.Setup(s => s[$"Census:Geocoding:{nameof(CensusGeocodingConfig.VintageId)}"]).Returns(value);

            // Test
            var censusGeocodingConfig = this.CreateCensusGeocodingConfig();
            var result = censusGeocodingConfig.VintageId;

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow(null, CensusGeocodingConfig.DefaultUrlFormatter)]
        [DataRow(" ", CensusGeocodingConfig.DefaultUrlFormatter)]
        [DataRow("test data", "test data")]
        public void UrlFormatterTest(string value, string expected)
        {
            // Stage

            // Mock
            mockConfiguration.Setup(s => s[$"Census:Geocoding:{nameof(CensusGeocodingConfig.UrlFormatter)}"]).Returns(value);

            // Test
            var censusGeocodingConfig = this.CreateCensusGeocodingConfig();
            var result = censusGeocodingConfig.UrlFormatter;

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
