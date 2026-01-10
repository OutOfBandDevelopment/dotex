using OoBDev.TestUtilities;
using OoBDev.Generations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests.Attributes
{
    [TestClass]
    public class GenerateEnumerationAttributeTests
    {
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private GenerateEnumerationAttribute CreateGenerateEnumerationAttribute()
        {
            return new GenerateEnumerationAttribute();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CanGenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateEnumerationAttribute = this.CreateGenerateEnumerationAttribute();
            IProcedualGenerationContext context = null;


            var result = generateEnumerationAttribute.CanGenerateValue(
                context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
