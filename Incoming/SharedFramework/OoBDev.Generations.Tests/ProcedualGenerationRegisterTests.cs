using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace OoBDev.Generations.Tests
{
    [TestClass]
    public class ProcedualGenerationRegisterTests
    {
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void EnsureExtensionTest()
        {
            //TODO: this is not reliable as other tests may have already added thes extensions
            //// Precheck -- these are not loaded
            //foreach (var ext in ProcedualGenerationRegister.ComponentModelExtensions)
            //    Assert.IsFalse(TypeDescriptor.GetAttributes(ext.type).Matches(ext.attribute), $"{ext.type} {ext.attribute}");

            ProcedualGenerationRegister.EnsureExtension();

            // Assert --- now they should be loaded

            foreach (var ext in ProcedualGenerationRegister.ComponentModelExtensions)
                Assert.IsTrue(TypeDescriptor.GetAttributes(ext.type).Matches(ext.attribute));
        }
    }
}
