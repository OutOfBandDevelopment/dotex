using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace OoBDev.Generations.Tests
{
    [TestClass]
    public class ProcedualGenerationContextTests
    {
#pragma warning disable CS8618 
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;
        private Mock<IProcedualGenerationProvider> mockProcedualGenerationProvider;

#pragma warning restore CS8618 

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockProcedualGenerationProvider = this.mockRepository.Create<IProcedualGenerationProvider>();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void TestCreated()
        {
            // Stage
            int seed = 543;
            var target = typeof(double);
            object reference = new object();

            var parameters = new object[] { 1, 2, 3 };
            var attributes = Enumerable.Empty<Attribute>();

            var expectedNext = 865841515;

            // Mock
            var mockParent = mockRepository.Create<IProcedualGenerationContext>();

            // Test
            var context = new ProcedualGenerationContext(
             seed,
             target,
             reference,
             parameters,
             attributes,
             mockParent.Object,
             this.mockProcedualGenerationProvider.Object
             );

            // Assert
            Assert.AreEqual(seed, context.Seed);
            Assert.AreEqual(expectedNext, context.Random.Next());
            Assert.AreEqual(target, context.TargetType);
            Assert.AreSame(reference, context.Reference);
            CollectionAssert.AreEquivalent(parameters, context.Parameters.ToList());
            Assert.AreSame(attributes, context.Attributes);
            Assert.AreSame(this.mockProcedualGenerationProvider.Object, context.Provider);
            Assert.AreSame(mockParent.Object, context.Parent);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
