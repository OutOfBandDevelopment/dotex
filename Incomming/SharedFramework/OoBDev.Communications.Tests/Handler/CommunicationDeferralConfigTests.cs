using OoBDev.TestUtilities;
using OoBDev.Communications.Handler;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Contracts;

namespace OoBDev.Communications.Tests.Handler
{
    [TestClass]
    public class CommunicationDeferralConfigTests
    {
        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private CommunicationDeferralConfig CreateCommunicationDeferralConfig()
        {
            return new CommunicationDeferralConfig(
                this.mockConfiguration.Object);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_Default()
        {
            // Stage
            string configValue = null;
            var expected = 100;

            // Mock
            mockConfiguration.Setup(m => m[CommunicationConfiguration.Deferral.MaxCount]).Returns(configValue);

            // Test
            var config = this.CreateCommunicationDeferralConfig();

            // Assert
            Assert.AreEqual(expected, config.MaxCount);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_MissConfigured()
        {
            // Stage
            string configValue = "bad value";
            var expected = 100;

            // Mock
            mockConfiguration.Setup(m => m[CommunicationConfiguration.Deferral.MaxCount]).Returns(configValue);

            // Test
            var config = this.CreateCommunicationDeferralConfig();

            // Assert
            Assert.AreEqual(expected, config.MaxCount);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_Configured()
        {
            // Stage
            string configValue = "5";
            var expected = 5;

            // Mock
            mockConfiguration.Setup(m => m[CommunicationConfiguration.Deferral.MaxCount]).Returns(configValue);

            // Test
            var config = this.CreateCommunicationDeferralConfig();

            // Assert
            Assert.AreEqual(expected, config.MaxCount);

            // Verify
            this.mockRepository.VerifyAll();
        }
        /*

            MaxCount = int.TryParse(config[CommunicationConfiguration.Deferral.MaxCount], out var mc) ?
                mc :
                CommunicationConfiguration.Deferral.MaxCountDefault;
        */
    }
}
