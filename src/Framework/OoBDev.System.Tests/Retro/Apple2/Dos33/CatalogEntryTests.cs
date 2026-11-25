using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Retro.Apple2.Dos33;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.System.Tests.Retro.Apple2.Dos33;

[TestClass]
public class CatalogEntryTests
{
    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void Constructor_SevenFileSlots_ReadsEachSlotAtItsOwnOffset()
    {
        // Stage
        var sector = new byte[256];
        sector[1] = 17;
        sector[2] = 14;
        for (var slot = 0; slot < 7; slot++)
        {
            var offset = 0x0b + (slot * 35);
            sector[offset] = 17;
            sector[offset + 1] = (byte)(slot + 1);
            sector[offset + 0x21] = (byte)(slot + 1);
        }

        // Test
        var catalog = new CatalogEntry(sector);

        // Assert
        Assert.AreEqual(17, catalog.NextCatalogTrack);
        Assert.AreEqual(14, catalog.NextSectorTrack);
        Assert.HasCount(7, catalog.FileEntries);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7 }, catalog.FileEntries.Select(e => (int)e.Sector).ToArray());
        Assert.IsTrue(catalog.FileEntries.All(e => e.Exists));
    }
}
