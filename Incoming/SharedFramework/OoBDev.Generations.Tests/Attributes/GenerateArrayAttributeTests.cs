using OoBDev.Generations.Attributes;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.Generations.Tests.Attributes
{
    [TestClass]
    public class GenerateArrayAttributeTests
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

        private GenerateArrayAttribute CreateGenerateArrayAttribute() =>
            new GenerateArrayAttribute();

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CanGenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateArrayAttribute = this.CreateGenerateArrayAttribute();
            IProcedualGenerationContext context = null;

            var result = generateArrayAttribute.CanGenerateValue(context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateArrayAttribute = this.CreateGenerateArrayAttribute();
            IProcedualGenerationContext context = null;

            var result = generateArrayAttribute.GenerateValue(context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
