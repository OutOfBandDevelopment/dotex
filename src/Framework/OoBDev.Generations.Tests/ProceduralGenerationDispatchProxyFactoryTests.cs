using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace OoBDev.Generations.Tests
{
    [TestClass]
    public class ProceduralGenerationDispatchProxyFactoryTests
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
        public void CreateTest_Interface()
        {
            // Stage
            var target = typeof(ITarget);

            // Mock
            var mockContext = mockRepository.Create<IProcedualGenerationContext>();
            mockContext.Setup(s=>s.TargetType).Returns(target);

            // Test
            var factory = new ProceduralGenerationDispatchProxyFactory();
            var result = factory.Create(mockContext.Object);

            // Assert
            Assert.IsInstanceOfType(result, target);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest_Class()
        {
            // Stage
            var target = typeof(Target);

            // Mock
            var mockContext = mockRepository.Create<IProcedualGenerationContext>();
            mockContext.Setup(s => s.TargetType).Returns(target);

            // Test
            var factory = new ProceduralGenerationDispatchProxyFactory();
            Assert.Throws<NotSupportedException>(() =>
            {
                var result = factory.Create(mockContext.Object);

                // Assert
                Assert.Fail("This is not supported so it should fail before you get here");
            });
        }

        public interface ITarget
        {
            int Name { get; set; }
        }
        public class Target : ITarget
        {
            public int Name { get; set; }
        }
    }
}
