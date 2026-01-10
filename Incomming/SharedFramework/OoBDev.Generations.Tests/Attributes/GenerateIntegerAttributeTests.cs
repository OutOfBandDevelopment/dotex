using OoBDev.TestUtilities;
using OoBDev.Generations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests.Attributes
{
    [TestClass]
    public class GenerateIntegerAttributeTests
    {
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private GenerateIntegerAttribute CreateGenerateIntegerAttribute()
        {
            return new GenerateIntegerAttribute();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CanGenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateIntegerAttribute = this.CreateGenerateIntegerAttribute();
            IProcedualGenerationContext context = null;


            var result = generateIntegerAttribute.CanGenerateValue(
                context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
