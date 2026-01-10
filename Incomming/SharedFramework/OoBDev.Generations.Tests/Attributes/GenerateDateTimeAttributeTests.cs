using OoBDev.TestUtilities;
using OoBDev.Generations.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests.Attributes
{
    [TestClass]
    public class GenerateDateTimeAttributeTests
    {
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private GenerateDateTimeAttribute CreateGenerateDateTimeAttribute()
        {
            return new GenerateDateTimeAttribute();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CanGenerateValueTest()
        {
            // Stage

            // Mock

            // Test
            var generateDateTimeAttribute = this.CreateGenerateDateTimeAttribute();
            IProcedualGenerationContext context = null;


            var result = generateDateTimeAttribute.CanGenerateValue(
                context);

            // Assert
            Assert.Fail();

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
