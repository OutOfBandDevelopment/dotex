using OoBDev.TestUtilities;
using OoBDev.IdentityModel.Contracts;
using OoBDev.IdentityModel.Extensions.Authorization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.IdentityModel.Tests.Authorization
{
    [TestClass]
    public class RightsProviderFactoryTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private RightsProviderFactory CreateFactory(params IRightsProvider[] providers) =>
            new RightsProviderFactory(providers);

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetRightsProvidersTest()
        {
            // Stage

            // Mock
            var mockIRightsProvider = mockRepository.Create<IRightsProvider>();

            // Test
            var factory = this.CreateFactory(mockIRightsProvider.Object);
            var result = factory.GetRightsProviders();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mockIRightsProvider.Object, result.Single());

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
