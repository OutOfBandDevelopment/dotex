using OoBDev.DocumentCenter.Contracts.Handlers;
using OoBDev.DocumentCenter.Contracts.Providers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;
using static OoBDev.DocumentCenter.Contracts.DocumentTypes;

namespace OoBDev.DocumentCenter.Tests.Providers
{
    [TestClass]
    public class DocumentConversionProviderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task Html2PdfTest_Direct()
        {
            var provider = this.TestContext.GetService<IDocumentConversionProvider>();
            var result = await provider.ConvertAsync(Html, Encoding.UTF8.GetBytes("<body><h1>Hello</h1><div>World!!</div><div>page 2</div></body>"), Pdf);
            this.TestContext?.WriteLine(Convert.ToBase64String(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task Markdown2HtmlTest_Direct()
        {
            var provider = this.TestContext.GetService<IDocumentConversionProvider>();
            var result = await provider.ConvertAsync(Markdown, Encoding.UTF8.GetBytes(@"# Hello World
## Summary

 This is a test!
"), Html);
            this.TestContext?.WriteLine(Encoding.UTF8.GetString(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task Markdown2PdfTest_Implicit()
        {
            var provider = this.TestContext.GetService<IDocumentConversionProvider>();
            var result = await provider.ConvertAsync(Markdown, Encoding.UTF8.GetBytes(@"# Hello World
## Summary

 This is a test!
"), Pdf);
            this.TestContext?.WriteLine(Convert.ToBase64String(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task Markdown2TextTest_Unhandled()
        {
            var provider = this.TestContext.GetService<IDocumentConversionProvider>();
            await Assert.ThrowsExceptionAsync<UnhandledConversionRequestedException>(async () =>
            {
                var result = await provider.ConvertAsync(Markdown, Encoding.UTF8.GetBytes(@"# Hello World
## Summary

 This is a test!
"), Text);
                this.TestContext?.WriteLine(Convert.ToBase64String(result));
            });
        }
    }
}
