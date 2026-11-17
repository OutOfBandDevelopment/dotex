using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Retro.Apple2.Dos33;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.Retro.Apple2.Dos33;

[TestClass]
public class DiskImageOrderTests
{
    private MemoryStream OpenImage()
    {
        using var source = this.GetResourceStream("1983_dos33c.dsk") ?? throw new global::System.NotSupportedException();
        var copy = new MemoryStream();
        source.CopyTo(copy);
        copy.Position = 0;
        return copy;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsProDosOrder_DosOrderImage_IsFalse()
    {
        using var image = OpenImage();

        Assert.IsFalse(DiskImageOrder.IsProDosOrder(image));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ConvertSectorOrder_DosImage_IsDetectedAsProDosAndReadsAfterConvertingBack()
    {
        using var image = OpenImage();
        using var proDos = DiskImageOrder.ConvertSectorOrder(image);

        Assert.IsTrue(DiskImageOrder.IsProDosOrder(proDos));
        using var back = DiskImageOrder.ConvertSectorOrder(proDos);
        CollectionAssert.AreEqual(image.ToArray(), back.ToArray());
        Assert.IsTrue(new DiskImageCommands().GetCatalogs(back).Any());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsProDosOrder_NotAWholeTrackImage_IsFalseAndConvertThrows()
    {
        using var odd = new MemoryStream(new byte[100000]);

        Assert.IsFalse(DiskImageOrder.IsProDosOrder(odd));
        Assert.ThrowsExactly<InvalidDataException>(() => DiskImageOrder.ConvertSectorOrder(odd));
    }
}