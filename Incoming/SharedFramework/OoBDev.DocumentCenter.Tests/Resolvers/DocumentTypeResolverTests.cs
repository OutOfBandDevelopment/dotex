using OoBDev.TestUtilities;
using OoBDev.DocumentCenter.Resolvers;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.DocumentCenter.Contracts;

namespace OoBDev.DocumentCenter.Tests.Resolvers
{
    [TestClass]
    public class DocumentTypeResolverTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IEnumTools> mockEnumTools;
        private Mock<IGuidTools> mockGuidTools;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockEnumTools = this.mockRepository.Create<IEnumTools>();
            this.mockGuidTools = this.mockRepository.Create<IGuidTools>();
        }

        private DocumentTypeResolver CreateDocumentTypeResolver() =>
            new DocumentTypeResolver(
                this.mockEnumTools.Object,
                this.mockGuidTools.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GenerateFileNameTest_ContentType()
        {
            // Stage
            var contentType = "test content type";
            var guid = Guid.Parse("18F52125-0337-4BDF-85F1-CBFB7DCF8C2D");
            var documentType = DocumentTypes.Xml;
            var fileExtension = new FileExtensionAttribute(".test");
            var expected = "JSH1GDcD30uF8cv7fc-MLQ.test";
            var doctypeAttributes = new[]
            {
                (documentType, new MimeTypeAttribute(contentType))
            };

            // Mock
            mockGuidTools.Setup(s => s.NewGuid()).Returns(guid);
            mockEnumTools.Setup(s => s.GetAttributes<DocumentTypes, MimeTypeAttribute>()).Returns(doctypeAttributes);
            mockEnumTools.Setup(s => s.GetAttributes<FileExtensionAttribute>(documentType)).Returns(new[] { fileExtension });

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GenerateFileName(contentType);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GenerateFileNameTest_DocumentTypes()
        {
            // Stage
            var guid = Guid.Parse("18F52125-0337-4BDF-85F1-CBFB7DCF8C2D");
            var documentType = DocumentTypes.Xml;
            var fileExtension = new FileExtensionAttribute(".test");
            var expected = "JSH1GDcD30uF8cv7fc-MLQ.test";

            // Mock
            mockGuidTools.Setup(s => s.NewGuid()).Returns(guid);
            mockEnumTools.Setup(s => s.GetAttributes<FileExtensionAttribute>(documentType)).Returns(new[] { fileExtension });

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GenerateFileName(documentType);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetByFileNameTest()
        {
            // Stage
            string fileName = "test-file-name.ext";
            var documentType = DocumentTypes.Xml;
            var enums = new[]
            {
                (documentType, new FileExtensionAttribute(".ext"))
            };

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<DocumentTypes, FileExtensionAttribute>()).Returns(enums);

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GetByFileName(fileName);

            // Assert
            Assert.AreEqual(documentType, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetByFileNameTest_Null()
        {
            // Stage

            // Mock

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            string fileName = null;


            var result = documentTypeResolver.GetByFileName(fileName);

            // Assert
            Assert.AreEqual(DocumentTypes.Unknown, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetByMimeTest()
        {
            // Stage
            var contentType = "test content type";
            var doctypeAttributes = new[]
            {
                (DocumentTypes.Text, new MimeTypeAttribute(contentType))
            };

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<DocumentTypes, MimeTypeAttribute>()).Returns(doctypeAttributes);

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GetByMime(contentType);

            // Assert
            Assert.AreEqual(DocumentTypes.Text, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetByMimeTest_Null()
        {
            // Stage

            // Mock

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GetByMime(null);

            // Assert
            Assert.AreEqual(DocumentTypes.Unknown, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetByMimeOrFileNameTest_ByContentType()
        {
            // Stage
            var contentType = "test content type";
            var fileName = "test file type";
            var doctypeAttributes = new[]
            {
                (DocumentTypes.Text, new MimeTypeAttribute(contentType))
            };

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<DocumentTypes, MimeTypeAttribute>()).Returns(doctypeAttributes);

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GetByMimeOrFileName(contentType, fileName);

            // Assert
            Assert.AreEqual(DocumentTypes.Text, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetByMimeOrFileNameTest_ByFileName()
        {
            // Stage
            var contentType = "test content type";
            var fileName = "test file type.ext";
            var documentType = DocumentTypes.Xml;
            var enums = new[]
            {
                (documentType, new FileExtensionAttribute(".ext"))
            };
            var doctypeAttributes = new[]
            {
                (DocumentTypes.Text, new MimeTypeAttribute("other"))
            };

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<DocumentTypes, MimeTypeAttribute>()).Returns(doctypeAttributes);
            mockEnumTools.Setup(s => s.GetAttributes<DocumentTypes, FileExtensionAttribute>()).Returns(enums);


            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GetByMimeOrFileName(contentType, fileName);

            // Assert
            Assert.AreEqual(documentType, result);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetDescriptionTest()
        {
            // Stage
            var documentType = DocumentTypes.Zip;
            var description = "test description";
            var attributes = new[] {
                new System.ComponentModel.DescriptionAttribute(description)
                };

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<System.ComponentModel.DescriptionAttribute>(documentType)).Returns(attributes);

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GetDescription(documentType);

            // Assert
            Assert.AreEqual(description, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetByPackageTypeTest()
        {
            // Stage
            var packageTypes = PackageTypes.ZipFile;
            var documentType = DocumentTypes.Zip;
            var attributes = new[] {
                new DocumentTypeAttribute(documentType)
                };

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<DocumentTypeAttribute>(packageTypes)).Returns(attributes);

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();

            var result = documentTypeResolver.GetByPackageType(packageTypes);

            // Assert
            Assert.AreEqual(documentType, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetExtensionTest()
        {
            // Stage
            var documentType = DocumentTypes.Xml;
            var fileExtension = new FileExtensionAttribute("test ext");

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<FileExtensionAttribute>(documentType)).Returns(new[] { fileExtension });

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();


            var result = documentTypeResolver.GetExtension(documentType);

            // Assert
            Assert.AreEqual(fileExtension.Extension, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetMimeTypeTest()
        {
            // Stage
            var documentType = DocumentTypes.Zip;
            var mime = "test description";
            var attributes = new[] {
                new MimeTypeAttribute(mime)
                };

            // Mock
            mockEnumTools.Setup(s => s.GetAttributes<MimeTypeAttribute>(documentType)).Returns(attributes);

            // Test
            var documentTypeResolver = this.CreateDocumentTypeResolver();
            var result = documentTypeResolver.GetMimeType(documentType);

            // Assert
            Assert.AreEqual(mime, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
