using OoBDev.IdentityModel.Contracts.Claims;
using OoBDev.IdentityModel.Extensions.Authorization;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Tests.Authorization
{
    [TestClass]
    public class ClaimsEnhancerFactoryTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private ClaimsEnhancerFactory CreateFactory(IEnumerable<IClaimsEnhancer> claimsEnhancers) =>
            new ClaimsEnhancerFactory(claimsEnhancers);

        public class ClaimsEnhancer1 : IClaimsEnhancer
        {
            public Task<JObject> EnhanceAsync(JObject claims)
            {
                claims[nameof(ClaimsEnhancer1)] = nameof(ClaimsEnhancer1);
                return Task.FromResult(claims);
            }
        }
        [ClaimsEnhancer]
        public class ClaimsEnhancer2 : IClaimsEnhancer
        {
            public Task<JObject> EnhanceAsync(JObject claims)
            {
                claims[nameof(ClaimsEnhancer2)] = nameof(ClaimsEnhancer2);
                return Task.FromResult(claims);
            }
        }
        [ClaimsEnhancer(Priority = 1)]
        public class ClaimsEnhancer3 : IClaimsEnhancer
        {
            public Task<JObject> EnhanceAsync(JObject claims)
            {
                claims[nameof(ClaimsEnhancer3)] = nameof(ClaimsEnhancer3);
                return Task.FromResult(claims);
            }
        }
        [ClaimsEnhancer(Priority = -1)]
        public class ClaimsEnhancer4 : IClaimsEnhancer
        {
            public Task<JObject> EnhanceAsync(JObject claims)
            {
                claims[nameof(ClaimsEnhancer4)] = nameof(ClaimsEnhancer4);
                return Task.FromResult(claims);
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEnhancersTest()
        {
            // Stage
            var items = new IClaimsEnhancer[]
            {
                new ClaimsEnhancer1(),
                new ClaimsEnhancer2(),
                new ClaimsEnhancer3(),
                new ClaimsEnhancer4()
            };

            // Mock

            // Test
            var factory = this.CreateFactory(items);
            var result = factory.GetEnhancers();

            // Assert
            var values = string.Join(";", result.Select(r=>r.GetType().Name));
            Assert.AreEqual("ClaimsEnhancer4;ClaimsEnhancer1;ClaimsEnhancer2;ClaimsEnhancer3", values);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
