using OoBDev.TestUtilities;
using OoBDev.Generations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests.Attributes
{
    [TestClass]
    public class GenerateLongAttributeTests
    {
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private GenerateLongAttribute CreateGenerateLongAttribute()
        {
            return new GenerateLongAttribute();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CanGenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateLongAttribute = this.CreateGenerateLongAttribute();
            IProcedualGenerationContext context = null;


            var result = generateLongAttribute.CanGenerateValue(
                context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
