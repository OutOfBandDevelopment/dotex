using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;

namespace OoBDev.Generations.Tests
{
    [TestClass]
    public class ProcedualGenerationContextBuilderTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TestContext? TestContext { get; set; }

        private MockRepository mockRepository;
        private Mock<IProcedualGenerationSeedGenerator> mockSeedGenerator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            mockSeedGenerator = mockRepository.Create<IProcedualGenerationSeedGenerator>();
        }

        private ProcedualGenerationContextBuilder CreateProcedualGenerationContextBuilder() =>
            new ProcedualGenerationContextBuilder(mockSeedGenerator.Object);

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateContextTest_ByType()
        {
            // Stage
            int index = 123;
            Type type = typeof(TargetObject);
            var attributes = Enumerable.Empty<Attribute>();

            var testSeed = 987;
            var expectedNext = 773155047;

            // Mock
            var mockprovider = mockRepository.Create<IProcedualGenerationProvider>();
            var mockContext = mockRepository.Create<IProcedualGenerationContext?>();

            mockSeedGenerator.Setup(s => s.Generate(mockContext.Object, index, type)).Returns(testSeed);

            // Test
            var procedualGenerationContextBuilder = this.CreateProcedualGenerationContextBuilder();

            var result = procedualGenerationContextBuilder.CreateContext(
                mockprovider.Object,
                mockContext.Object,
                index,
                type,
                attributes
                );

            // Assert
            Assert.AreEqual(testSeed, result.Seed);
            Assert.AreSame(type, result.TargetType);
            Assert.AreSame(mockprovider.Object, result.Provider);
            Assert.AreSame(attributes, result.Attributes);
            Assert.AreSame(mockContext.Object, result.Parent);

            Assert.AreEqual(expectedNext, result.Random.Next());

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateContextTest_ByMethod()
        {
            // Stage
            int index = 123;
            var attributes = Enumerable.Empty<Attribute>();

            Type type = typeof(TargetObject);
            var method = type.GetMethod(nameof(TargetObject.Function)) ?? throw new NotSupportedException();
            var arguments = new object[] { 1.2 };

            var testSeed = 987;
            var expectedNext = 773155047;

            // Mock
            var mockprovider = mockRepository.Create<IProcedualGenerationProvider>();
            var mockContext = mockRepository.Create<IProcedualGenerationContext?>();

            mockSeedGenerator.Setup(s => s.Generate(mockContext.Object, index, method, arguments)).Returns(testSeed);

            // Test
            var procedualGenerationContextBuilder = this.CreateProcedualGenerationContextBuilder();

            var result = procedualGenerationContextBuilder.CreateContext(
                mockprovider.Object,
                mockContext.Object,
                index,
                method,
                arguments,
                attributes);

            // Assert
            Assert.AreEqual(testSeed, result.Seed);
            Assert.AreSame(method.ReturnType, result.TargetType);
            Assert.AreSame(mockprovider.Object, result.Provider);
            Assert.AreSame(mockContext.Object, result.Parent);

            Assert.AreEqual(expectedNext, result.Random.Next());

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateContextTest_ByConstructor()
        {
            // Stage
            int index = 123;
            var attributes = Enumerable.Empty<Attribute>();

            Type type = typeof(TargetObject);
            var method = type.GetConstructor(new[] {typeof(int)}) ?? throw new NotSupportedException();
            var arguments = new object[] { 345 };

            var testSeed = 987;
            var expectedNext = 773155047;

            // Mock
            var mockprovider = mockRepository.Create<IProcedualGenerationProvider>();
            var mockContext = mockRepository.Create<IProcedualGenerationContext?>();

            mockSeedGenerator.Setup(s => s.Generate(mockContext.Object, index, method, arguments)).Returns(testSeed);

            // Test
            var procedualGenerationContextBuilder = this.CreateProcedualGenerationContextBuilder();

            var result = procedualGenerationContextBuilder.CreateContext(
                mockprovider.Object,
                mockContext.Object,
                index,
                method,
                arguments,
                attributes);

            // Assert
            Assert.AreEqual(testSeed, result.Seed);
            Assert.AreSame(type, result.TargetType);
            Assert.AreSame(mockprovider.Object, result.Provider);
            Assert.AreSame(mockContext.Object, result.Parent);

            Assert.AreEqual(expectedNext, result.Random.Next());

            // Verify
            this.mockRepository.VerifyAll();
        }

        public class TargetObject
        {
            public TargetObject(int input) { }
            public string Function(double input) => "Hello World!";
        }

    }
}
