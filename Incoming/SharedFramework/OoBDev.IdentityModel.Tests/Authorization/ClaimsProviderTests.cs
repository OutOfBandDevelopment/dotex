using OoBDev.TestUtilities;
using OoBDev.IdentityModel.Extensions.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OoBDev.IdentityModel.Tests.Authorization
{
    [TestClass]
    public class ClaimsProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IClaimsEnhancerPipeline> mockClaimsEnhancerPipeline;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockClaimsEnhancerPipeline = this.mockRepository.Create<IClaimsEnhancerPipeline>();
        }

        private ClaimsProvider CreateProvider() =>
            new ClaimsProvider(
                this.mockClaimsEnhancerPipeline.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task GetAdditionalClaimsAsyncTest()
        {
            // Stage
            var claims = new JObject();

            // Mock
            mockClaimsEnhancerPipeline.Setup(s => s.EnhanceAsync(claims)).ReturnsAsync(claims);

            // Test
            var provider = this.CreateProvider();
            var result = await provider.GetAdditionalClaimsAsync(claims);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
