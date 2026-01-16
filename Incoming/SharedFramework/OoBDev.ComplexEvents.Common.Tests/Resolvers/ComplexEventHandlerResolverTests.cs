using OoBDev.ComplexEvents.Common.Resolvers;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Resolvers
{
    [TestClass]
    public class ComplexEventHandlerResolverTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private ComplexEventHandlerResolver CreateComplexEventHandlerResolver() => 
            new ComplexEventHandlerResolver(
                this.TestContext.GetTestLoggingServices<ComplexEventHandlerResolver>()
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_NoTarget()
        {
            // Stage
            string target = null;

            // Mock
            var mockHandler = mockRepository.Create<IComplexEventHandler>();

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, mockHandler.Object.GetType());

            // Assert
            Assert.IsFalse(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_NoHandler()
        {
            // Stage
            string target = null;
            IComplexEventHandler handler = null;

            // Mock

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, handler?.GetType());

            // Assert
            Assert.IsFalse(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_NoAttribute()
        {
            // Stage
            string target = "find me";
            IComplexEventHandler handler = new TestTarget_NoAttribute();

            // Mock

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, handler.GetType());

            // Assert
            Assert.IsTrue(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_AttributeNoValue()
        {
            // Stage
            string target = "find me";
            IComplexEventHandler handler = new TestTarget_AttributeNoValue();

            // Mock

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, handler.GetType());

            // Assert
            Assert.IsTrue(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_AttributeTargetType()
        {
            // Stage
            string target = typeof(TestEvent).FullName;
            IComplexEventHandler handler = new TestTarget_AttributeTargetType();

            // Mock

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, handler.GetType());

            // Assert
            Assert.IsTrue(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_AttributeTargetType_ClrObject()
        {
            // Stage
            string target = $"clr/object+{typeof(TestEvent).FullName}".ToUpper();
            IComplexEventHandler handler = new TestTarget_AttributeTargetType();

            // Mock

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, handler.GetType());

            // Assert
            Assert.IsTrue(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_AttributeTargetType_NoMatch()
        {
            // Stage
            string target = $"{typeof(TestEvent).FullName}XXX".ToUpper();
            IComplexEventHandler handler = new TestTarget_AttributeTargetType();

            // Mock

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, handler.GetType());

            // Assert
            Assert.IsFalse(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CheckTargetTest_AttributeTargetTypeExtended()
        {
            // Stage
            string target = $"clr/object+{typeof(TestEvent).FullName}";
            IComplexEventHandler handler = new TestTarget_AttributeTargetType();

            // Mock

            // Test
            var complexEventHandlerResolver = this.CreateComplexEventHandlerResolver();
            var result = complexEventHandlerResolver.CheckTarget(target, handler.GetType());

            // Assert
            Assert.IsTrue(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        public class TestTarget_NoAttribute : IComplexEventHandler
        {
            public Task HandleEvent(object message) => throw new NotImplementedException();
        }
        [ComplexEventHandler]
        public class TestTarget_AttributeNoValue : IComplexEventHandler
        {
            public Task HandleEvent(object message) => throw new NotImplementedException();
        }
        [ComplexEventHandler(TargetType = typeof(TestEvent))]
        public class TestTarget_AttributeTargetType : IComplexEventHandler
        {
            public Task HandleEvent(object message) => throw new NotImplementedException();
        }

        public class TestEvent
        {
        }
    }
}
