using OoBDev.IdentityModel.Contracts.Claims;
using OoBDev.IdentityModel.Contracts.Handlers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace OoBDev.IdentityModel.Tests.Authorization
{
    [TestClass]
    public class ClaimsProviderIntegrationTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public async Task GetAdditionalClaimsAsyncTest()
        {
            var claims = this.TestContext.GetService<IClaimsProvider>();

            var data = new JObject
            {
                [AzB2cClaims.ObjectId] = "SYSTEM"
            };

            var result = await claims.GetAdditionalClaimsAsync(data);

            this.TestContext.AddResult(result);
        }
    }
}
