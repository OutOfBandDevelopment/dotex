using OoBDev.TestUtilities;
using OoBDev.Communications.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.OoBDev.Persistence.Contracts.Models.Test;
using OoBDev.Communications.Contracts;

namespace OoBDev.Communications.Tests.Services
{
    [TestClass]
    public class AttributeResolverTests
    {
        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }
        private AttributeResolver CreateMessageTypeResolver() => new AttributeResolver();

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetMessageTypeTest_NoAttribute()
        {
            // Stage
            var expected = "Test_Test_TargetClassNoAttribute";

            // Mock

            // Test
            var messageTypeResolver = this.CreateMessageTypeResolver();

            var result = messageTypeResolver.GetMessageType<TargetClassNoAttribute>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetMessageTypeTest_EmptyAttribute()
        {
            // Stage
            var expected = "Test_Test_TargetClassEmptyAttribute";

            // Mock

            // Test
            var messageTypeResolver = this.CreateMessageTypeResolver();

            var result = messageTypeResolver.GetMessageType<TargetClassEmptyAttribute>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetMessageTypeTest_WithAttribute()
        {
            // Stage
            var expected = "Test MessageType";

            // Mock

            // Test
            var messageTypeResolver = this.CreateMessageTypeResolver();

            var result = messageTypeResolver.GetMessageType<TargetClassWithAttribute>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetPriorityTest_Normal()
        {
            // Stage
            var expected = RequestPriorities.Normal;

            // Mock

            // Test
            var messageTypeResolver = this.CreateMessageTypeResolver();

            var result = messageTypeResolver.GetPriority<TargetClassWithAttributeNormal>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetPriorityTest_NoAttribute()
        {
            // Stage
            var expected = RequestPriorities.Normal;

            // Mock

            // Test
            var messageTypeResolver = this.CreateMessageTypeResolver();

            var result = messageTypeResolver.GetPriority<TargetClassNoAttribute>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetPriorityTest_Default()
        {
            // Stage
            var expected = RequestPriorities.Normal;

            // Mock

            // Test
            var messageTypeResolver = this.CreateMessageTypeResolver();

            var result = messageTypeResolver.GetPriority<TargetClassEmptyAttribute>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public void GetPriorityTest_Immediate()
        {
            // Stage
            var expected = RequestPriorities.Immediate;

            // Mock

            // Test
            var messageTypeResolver = this.CreateMessageTypeResolver();

            var result = messageTypeResolver.GetPriority<TargetClassWithAttributeImmediate>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
namespace Test.OoBDev.Persistence.Contracts.Models.Test
{
    public class TargetClassNoAttribute { }
    [Communication]
    public class TargetClassEmptyAttribute { }
    [Communication(MessageType = "Test MessageType")]
    public class TargetClassWithAttribute { }

    [Communication(Priority = RequestPriorities.Normal)]
    public class TargetClassWithAttributeNormal { }

    [Communication(Priority = RequestPriorities.Immediate)]
    public class TargetClassWithAttributeImmediate { }
}