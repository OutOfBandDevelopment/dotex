using OoBDev.TestUtilities;
using OoBDev.Generations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests.Attributes
{
    [TestClass]
    public class GenerateInterfaceAttributeTests
    {
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private GenerateInterfaceAttribute CreateGenerateInterfaceAttribute()
        {
            return new GenerateInterfaceAttribute();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CanGenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateInterfaceAttribute = this.CreateGenerateInterfaceAttribute();
            IProcedualGenerationContext context = null;


            var result = generateInterfaceAttribute.CanGenerateValue(
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
            var generateInterfaceAttribute = this.CreateGenerateInterfaceAttribute();
            IProcedualGenerationContext context = null;


            var result = generateInterfaceAttribute.GenerateValue(
                context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
