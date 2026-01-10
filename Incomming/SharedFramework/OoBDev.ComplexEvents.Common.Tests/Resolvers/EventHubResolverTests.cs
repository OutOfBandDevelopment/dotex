using OoBDev.TestUtilities;
using OoBDev.ComplexEvents.Common.Resolvers;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.ComplexEvents.Contracts;
using ComplexEvents.Tests;

namespace OoBDev.ComplexEvents.Common.Tests.Resolvers
{
    [TestClass]
    public class EventHubResolverTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
        }

        private EventHubResolver CreateEventHubResolver() =>
            new EventHubResolver(
                this.mockConfiguration.Object);

        public class TestTarget_NoAttribute
        {
        }
        [EventHubChannel]
        public class TestTarget_AttributeNoValue
        {
        }
        [EventHubChannel(EventHubName = TestHubName, PartitionKey = TestKeyName)]
        public class TestTarget_AttributeValue
        {
            public const string TestHubName = "test hub value";
            public const string TestKeyName = "test key value";
        }

        [EventHubChannel(EventHubName = TestHubName, PartitionKey = TestKeyName)]
        public class TestTarget_AttributeValue<T>
        {
            public const string TestHubName = "test hub generic";
            public const string TestKeyName = "test key generic";
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEventHubNameTest_NoAttribute()
        {
            // Stage
            var expected = "lvers.EventHubResolverTests+TestTarget_NoAttribute";

            // Mock
            mockConfiguration.Setup(s => s[EventHubResolver.DefaultHubNameConfigKey]).Returns(string.Empty);

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetEventHubName<TestTarget_NoAttribute>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEventHubNameTest_AttributeValue()
        {
            // Stage
            var expected = TestTarget_AttributeValue.TestHubName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetEventHubName<TestTarget_AttributeValue>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEventHubNameTest_GenericAttributeValue()
        {
            // Stage
            var expected = TestTarget_AttributeValue<string>.TestHubName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetEventHubName<TestTarget_AttributeValue<string>>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEventHubNameTest_AttributeNoValue_NoDefault()
        {
            // Stage
            var expected = ".EventHubResolverTests+TestTarget_AttributeNoValue";

            // Mock
            mockConfiguration.Setup(s => s[EventHubResolver.DefaultHubNameConfigKey]).Returns(string.Empty);

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetEventHubName<TestTarget_AttributeNoValue>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEventHubNameTest_AttributeNoValue_WithDefault()
        {
            // Stage
            var expected = "test hub value";
            var defaultValue = expected;

            // Mock
            mockConfiguration.Setup(s => s[EventHubResolver.DefaultHubNameConfigKey]).Returns(defaultValue);

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetEventHubName<TestTarget_AttributeNoValue>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEventHubNameTest_Short()
        {
            // Stage
            var expected = typeof(TestTarget_Short).FullName;
            var defaultValue = expected;

            // Mock
            mockConfiguration.Setup(s => s[EventHubResolver.DefaultHubNameConfigKey]).Returns(defaultValue);

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetEventHubName<TestTarget_Short>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetEventHubNameTest_GenericShort()
        {
            // Stage
            var expected = typeof(TestTarget_Short<>).FullName;
            var defaultValue = expected;

            // Mock
            mockConfiguration.Setup(s => s[EventHubResolver.DefaultHubNameConfigKey]).Returns(defaultValue);

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetEventHubName<TestTarget_Short<string>>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetPartitionKeyTest_NoAttribute()
        {
            // Stage
            var expected = typeof(TestTarget_NoAttribute).FullName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetPartitionKey<TestTarget_NoAttribute>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetPartitionKeyTest_AttributeValue()
        {
            // Stage
            var expected = TestTarget_AttributeValue.TestKeyName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetPartitionKey<TestTarget_AttributeValue>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetPartitionKeyTest_GenericAttributeValue()
        {
            // Stage
            var expected = TestTarget_AttributeValue<string>.TestKeyName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetPartitionKey<TestTarget_AttributeValue<string>>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetPartitionKeyTest_AttributeNoValue()
        {
            // Stage
            var expected = typeof(TestTarget_AttributeNoValue).FullName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetPartitionKey<TestTarget_AttributeNoValue>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetPartitionKeyTest_Short()
        {
            // Stage
            var expected = typeof(TestTarget_Short).FullName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetPartitionKey<TestTarget_Short>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify            
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetPartitionKeyTest_GenericShort()
        {
            // Stage
            var expected = typeof(TestTarget_Short<>).FullName;

            // Mock

            // Test
            var eventHubResolver = this.CreateEventHubResolver();
            var result = eventHubResolver.GetPartitionKey<TestTarget_Short<string>>();
            this.TestContext?.WriteLine(result);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
namespace ComplexEvents.Tests
{
    public class TestTarget_Short
    {
    }
    public class TestTarget_Short<T>
    {
    }
}
