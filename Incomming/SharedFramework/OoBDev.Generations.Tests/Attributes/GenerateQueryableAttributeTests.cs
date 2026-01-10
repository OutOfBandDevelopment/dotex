using OoBDev.TestUtilities;
using OoBDev.Generations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests.Attributes
{
    [TestClass]
    public class GenerateQueryableAttributeTests
    {
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private GenerateQueryableAttribute CreateGenerateQueryableAttribute()
        {
            return new GenerateQueryableAttribute();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CanGenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateQueryableAttribute = this.CreateGenerateQueryableAttribute();
            IProcedualGenerationContext context = null;


            var result = generateQueryableAttribute.CanGenerateValue(
                context);

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
            var generateQueryableAttribute = this.CreateGenerateQueryableAttribute();
            IProcedualGenerationContext context = null;


            var result = generateQueryableAttribute.GenerateValue(
                context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
