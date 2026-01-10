using OoBDev.DocumentCenter.Storage;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OoBDev.DocumentCenter.Tests.Storage
{
    [TestClass]
    public class DocumentKeyGeneratorTests
    {
        public TestContext TestContext { get; set; }
        [TestMethod, TestCategory(TestCategories.Unit)]
        public void GenerateTest()
        {
            var ts = DateTimeOffset.Now;

            var target = new DocumentKeyGenerator();
            var result = target.Generate("test.pdf", ts);
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Format("test_{0}.pdf", ts.Ticks), result);

            this.TestContext?.WriteLine($"File Key: {result}");
        }
    }
}
