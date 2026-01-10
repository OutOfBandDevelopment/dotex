using OoBDev.Generations.Extensions.DependencyInjection;
using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Generations.Tests.Extensions.DependencyInjection
{
    [TestClass]
    public class ServiceCollectionExtensionsTests
    {
#pragma warning disable CS8618 
        public TestContext? TestContext { get; set; }
#pragma warning restore CS8618

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddProcedualGenerationServicesTest()
        {
            // Stage
            var services = new ServiceCollection();

            // Test
            var result = services.AddProcedualGenerationServices();

            // Assert
            Assert.AreSame(services, result);

            var built = services.BuildServiceProvider();
            var builder = built.GetService<IProcedualGenerationProviderBuilder>();

            Assert.IsInstanceOfType(builder, typeof(ProcedualGenerationProviderBuilder));
        }
    }
}
