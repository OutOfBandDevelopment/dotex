using OoBDev.Api.Microsoft.Caching.Providers;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace OoBDev.Api.Microsoft.Caching.Tests.Providers
{
    [TestClass]
    public class MicrosoftMemoryCachingProviderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        public async Task TestAll()
        {
            // Stage

            // Mock

            var services = new ServiceCollection()
                .AddTransient<IConfiguration>(sp => null)
                .AddOptions()
                .BuildServiceProvider();
            var options = services.GetRequiredService<IOptions<MemoryCacheOptions>>();

            // Test
            using (var provider = new MicrosoftMemoryCachingProvider(options))
            {
                var testKey = Guid.NewGuid().ToString();
                var testValue = Guid.NewGuid();

                var result1 = await provider.RetreiveAsync(testKey, typeof(Guid));
                Assert.IsNull(result1);

                await provider.StoreAsync(testKey, testValue, new TimeSpan(0, 0, 0, 0, 200));

                var result2 = await provider.RetreiveAsync(testKey, typeof(Guid));
                Assert.IsNotNull(result2);
                Assert.AreEqual(testValue, result2);

                await Task.Delay(300);

                var result3 = await provider.RetreiveAsync(testKey, typeof(Guid));
                Assert.IsNull(result3);

                await provider.StoreAsync(testKey, testValue, new TimeSpan(0, 1, 0));

                var result4 = await provider.RetreiveAsync(testKey, typeof(Guid));
                Assert.IsNotNull(result4);
                Assert.AreEqual(testValue, result4);

                await provider.FlushAsync(testKey);

                var result5 = await provider.RetreiveAsync(testKey, typeof(Guid));
                Assert.IsNull(result5);

            }


            // Assert

            // Verify
        }

    }
}
