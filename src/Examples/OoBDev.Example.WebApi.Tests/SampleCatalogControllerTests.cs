using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class SampleCatalogControllerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Query_Default_ReturnsTheTenRowsAsAQueryable()
    {
        // Test
        var rows = new SampleCatalogController().Query();
        TestContext.WriteLine($"rows={rows.Count()} categories={string.Join(",", rows.Select(r => r.Category).Distinct())}");

        // Assert
        Assert.AreEqual(10, rows.Count());
        CollectionAssert.AreEquivalent(new[] { "Parts", "Tools", "Instruments" }, rows.Select(r => r.Category).Distinct().ToArray());
    }
}
