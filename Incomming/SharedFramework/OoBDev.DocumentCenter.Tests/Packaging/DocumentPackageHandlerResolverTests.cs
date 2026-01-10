using OoBDev.TestUtilities;
using OoBDev.DocumentCenter.Packaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.DocumentCenter.Contracts;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.DocumentCenter.Contracts.Handlers;
using System.Threading.Tasks;
using System.Collections.Generic;
using OoBDev.DocumentCenter.Contracts.Storage;

namespace OoBDev.DocumentCenter.Tests.Packaging
{
    [TestClass]
    public class DocumentPackageHandlerResolverTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private DocumentPackageHandlerResolver CreateDocumentPackageHandlerResolver(IServiceProvider serviceProvider)
            => new DocumentPackageHandlerResolver(serviceProvider);

        [PackageHandler(PackageTypes.ZipFile, Priority = int.MaxValue)]
        public class TestHandler : IDocumentPackageHandler
        {
            public Task<byte[]> PackageAsync(IEnumerable<IDocumentRequestReference> documents)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetHandlerTest()
        {
            // Stage
            var packageType = PackageTypes.ZipFile;
            var serviceProvider = new ServiceCollection()
                .AddTransient<IDocumentPackageHandler, TestHandler>()
                .BuildServiceProvider()
                ;

            // Mock

            // Test
            var documentPackageHandlerResolver = this.CreateDocumentPackageHandlerResolver(serviceProvider);
            var result = documentPackageHandlerResolver.GetHandler(packageType);

            // Assert
            Assert.IsInstanceOfType(result, typeof(TestHandler));

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
