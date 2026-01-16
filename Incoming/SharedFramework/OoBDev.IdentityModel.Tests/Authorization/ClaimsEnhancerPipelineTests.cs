using OoBDev.TestUtilities;
using OoBDev.IdentityModel.Extensions.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OoBDev.IdentityModel.Contracts.Claims;

namespace OoBDev.IdentityModel.Tests.Authorization
{
    [TestClass]
    public class ClaimsEnhancerPipelineTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IClaimsEnhancerFactory> mockClaimsEnhancerFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockClaimsEnhancerFactory = this.mockRepository.Create<IClaimsEnhancerFactory>();
        }

        private ClaimsEnhancerPipeline CreateClaimsEnhancerPipeline() =>
            new ClaimsEnhancerPipeline(
                this.mockClaimsEnhancerFactory.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task EnhanceAsyncTest()
        {
            // Stage
            var claims = new JObject();

            // Mock
            var mockClaimsEnhancer = mockRepository.Create<IClaimsEnhancer>();
            mockClaimsEnhancerFactory.Setup(s => s.GetEnhancers()).Returns(new[]
            {
                mockClaimsEnhancer.Object,
            });

            mockClaimsEnhancer.Setup(s => s.EnhanceAsync(claims)).ReturnsAsync(claims);

            // Test
            var claimsEnhancerPipeline = this.CreateClaimsEnhancerPipeline();
            var result = await claimsEnhancerPipeline.EnhanceAsync(claims);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
