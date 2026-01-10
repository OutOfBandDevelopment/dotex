using OoBDev.TestUtilities;
using OoBDev.DocumentCenter.Resolvers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.DocumentCenter.Contracts.Storage;
using shortpath;

namespace OoBDev.DocumentCenter.Tests.Resolvers
{
    [TestClass]
    public class BlobContainerResolverTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;



        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


        }

        private BlobContainerResolver CreateBlobContainerResolver() => new BlobContainerResolver();

        [BlobContainer(ContainerName = "Test Container")]
        public class TestTaggedContainer
        {
        }
        [BlobContainer]
        public class TestContainer
        {
        }

        [BlobContainer]
        public class TestContainerGeneric<T>
        {
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetContainerNameTest_Tagged()
        {
            // Stage
            var expected = "Test Container";

            // Mock

            // Test
            var blobContainerResolver = this.CreateBlobContainerResolver();


            var result = blobContainerResolver.GetContainerName<TestTaggedContainer>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetContainerNameTest_NotTagged()
        {
            // Stage
            var expected = "center-tests-resolvers-blobcontainerresolvertests-testcontainer";

            // Mock

            // Test
            var blobContainerResolver = this.CreateBlobContainerResolver();


            var result = blobContainerResolver.GetContainerName<TestContainer>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetContainerNameTest_Generic()
        {
            // Stage
            var expected = "sts-resolvers-blobcontainerresolvertests-testcontainergeneric-1";

            // Mock

            // Test
            var blobContainerResolver = this.CreateBlobContainerResolver();


            var result = blobContainerResolver.GetContainerName<TestContainerGeneric<TestTaggedContainer>>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetContainerNameTest_ShortNamespace()
        {
            // Stage
            var expected = "shortpath-testcontainershort";

            // Mock

            // Test
            var blobContainerResolver = this.CreateBlobContainerResolver();


            var result = blobContainerResolver.GetContainerName<TestContainerShort>();

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }

}

namespace shortpath
{
    [BlobContainer]
    public class TestContainerShort
    {
    }
}