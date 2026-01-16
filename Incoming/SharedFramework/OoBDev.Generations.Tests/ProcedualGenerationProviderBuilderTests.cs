using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests
{
    [TestClass]
    public class ProcedualGenerationProviderBuilderTests
    {
#pragma warning disable CS8618 
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;
#pragma warning restore CS8618 

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void BuildTest_BuildConstructor()
        {
            // Stage
            var targetInterface = typeof(IProcedualGenerationProvider);

            // Mock
            var mockServiceProvider = mockRepository.Create<IServiceProvider>();

            mockServiceProvider.Setup(i => i.GetService(typeof(IProcedualGenerationContextBuilder))).Returns<IProcedualGenerationContextBuilder?>(default);
            mockServiceProvider.Setup(i => i.GetService(typeof(IProceduralGenerationDispatchProxyFactory))).Returns<IProceduralGenerationDispatchProxyFactory?>(default);
            mockServiceProvider.Setup(i => i.GetService(typeof(IProceduralGenerationTypeBuilderFactory))).Returns<IProceduralGenerationTypeBuilderFactory?>(default);

            // Test

            var builder = new ProcedualGenerationProviderBuilder(mockServiceProvider.Object);
            var result = builder.Build();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ProcedualGenerationProvider));
            Assert.AreEqual(mockServiceProvider.Object, result.ServiceProvider);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
