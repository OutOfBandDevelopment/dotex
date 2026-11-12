using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Retro.Apple2.Dos33;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OoBDev.System.Tests.Retro.Apple2.Dos33;

[TestClass]
public class DiskImageWriterTests
{
    private MemoryStream OpenImage()
    {
        using var source = this.GetResourceStream("1983_dos33c.dsk") ?? throw new NotSupportedException();
        var copy = new MemoryStream();
        source.CopyTo(copy);
        return copy;
    }

    private static int FreeSectors(Stream image)
    {
        var vtoc = new DiskImageCommands().GetVolumeTableOfContents(image);
        return vtoc.BitmapFreeSectors.Take(vtoc.NumberOfTracksPerDisk).Sum(b => BitOperations.PopCount(b & 0xFFFF));
    }

    private static FileEntry Find(Stream image, string name) =>
        new DiskImageCommands().GetCatalogs(image).SelectMany(c => c.FileEntries)
            .Single(e => e.Exists && e.Name.Trim() == name);

    private static byte[] Pattern(int length) => [.. Enumerable.Range(0, length).Select(i => (byte)((i * 7) + 1))];

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_SmallFile_ReadsBackThroughTheReader()
    {
        using var image = OpenImage();
        var data = Pattern(300);

        new DiskImageWriter().AddFile(image, "readback", AppleFileType.Binary, data);

        var entry = Find(image, "READBACK");
        Assert.AreEqual(AppleFileType.Binary, entry.FileType);
        Assert.AreEqual(3, entry.FileSize);
        var read = new DiskImageCommands().GetDataFileEntry(image, entry).ToArray();
        Assert.AreEqual(512, read.Length);
        CollectionAssert.AreEqual(data, read[..300]);
        Assert.IsTrue(read[300..].All(b => b == 0));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_File_TakesListAndDataSectorsFromTheBitmap()
    {
        using var image = OpenImage();
        var before = FreeSectors(image);

        new DiskImageWriter().AddFile(image, "SIZE", AppleFileType.Text, Pattern(1000));

        Assert.AreEqual(before - 5, FreeSectors(image));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetDataFileEntry_FileOverOneListSector_ReadsEverySectorInOrder()
    {
        using var image = OpenImage();
        var data = Pattern(130 * 256);
        new DiskImageWriter().AddFile(image, "BIG", AppleFileType.Binary, data);

        var read = new DiskImageCommands().GetDataFileEntry(image, Find(image, "BIG"));

        CollectionAssert.AreEqual(data, read.ToArray());
    }
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_FileOverOneListSector_ChainsTheLists()
    {
        using var image = OpenImage();
        var before = FreeSectors(image);
        var data = Pattern(130 * 256);

        new DiskImageWriter().AddFile(image, "BIG", AppleFileType.Binary, data);

        Assert.AreEqual(before - 132, FreeSectors(image));
        var entry = Find(image, "BIG");
        Assert.AreEqual(132, entry.FileSize);
        var bytes = image.ToArray();
        var first = ((entry.Track * 16) + entry.Sector) * 256;
        Assert.AreNotEqual(0, bytes[first + 1]);
        var second = ((bytes[first + 1] * 16) + bytes[first + 2]) * 256;
        Assert.AreEqual(122, bytes[second + 5]);
        Assert.AreEqual(0, bytes[second + 1]);
        var lastData = ((bytes[second + 0x0C + (7 * 2)] * 16) + bytes[second + 0x0D + (7 * 2)]) * 256;
        CollectionAssert.AreEqual(data[(129 * 256)..], bytes[lastData..(lastData + 256)]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_ExistingNameWithoutReplace_Throws()
    {
        using var image = OpenImage();
        var writer = new DiskImageWriter();
        writer.AddFile(image, "TWICE", AppleFileType.Text, Pattern(10));

        Assert.ThrowsExactly<InvalidOperationException>(() => writer.AddFile(image, "twice", AppleFileType.Text, Pattern(10)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_Replace_SwapsTheContentAndFreesTheOldSectors()
    {
        using var image = OpenImage();
        var writer = new DiskImageWriter();
        writer.AddFile(image, "SWAP", AppleFileType.Text, Pattern(1000));
        var afterFirst = FreeSectors(image);

        writer.AddFile(image, "SWAP", AppleFileType.Text, Pattern(10), replace: true);

        Assert.AreEqual(afterFirst + 3, FreeSectors(image));
        var entry = Find(image, "SWAP");
        CollectionAssert.AreEqual(Pattern(10), new DiskImageCommands().GetDataFileEntry(image, entry).ToArray()[..10]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_DiskFull_ThrowsAndLeavesTheImageUnchanged()
    {
        using var image = OpenImage();
        var before = image.ToArray();

        Assert.ThrowsExactly<InvalidOperationException>(() => new DiskImageWriter().AddFile(image, "HUGE", AppleFileType.Binary, new byte[143360]));

        CollectionAssert.AreEqual(before, image.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void DeleteFile_ExistingFile_FreesSectorsAndMarksTheEntryDeleted()
    {
        using var image = OpenImage();
        var before = FreeSectors(image);
        var writer = new DiskImageWriter();
        writer.AddFile(image, "GONE", AppleFileType.Text, Pattern(600));
        var track = Find(image, "GONE").Track;

        var deleted = writer.DeleteFile(image, "gone");

        Assert.IsTrue(deleted);
        Assert.AreEqual(before, FreeSectors(image));
        var entry = new DiskImageCommands().GetCatalogs(image).SelectMany(c => c.FileEntries).Single(e => e.IsDeleted && e.OriginalTrack == track);
        Assert.IsFalse(entry.Exists);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void DeleteFile_MissingFile_ReturnsFalse()
    {
        using var image = OpenImage();
        var before = image.ToArray();

        Assert.IsFalse(new DiskImageWriter().DeleteFile(image, "NOPE"));

        CollectionAssert.AreEqual(before, image.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_InvalidName_Throws()
    {
        using var image = OpenImage();
        var writer = new DiskImageWriter();

        Assert.ThrowsExactly<ArgumentException>(() => writer.AddFile(image, string.Empty, AppleFileType.Text, default));
        Assert.ThrowsExactly<ArgumentException>(() => writer.AddFile(image, new string('A', 31), AppleFileType.Text, default));
        Assert.ThrowsExactly<ArgumentException>(() => writer.AddFile(image, "A,B", AppleFileType.Text, default));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddFile_NotADosImage_ThrowsInvalidData()
    {
        using var zeros = new MemoryStream(new byte[143360]);
        using var tiny = new MemoryStream(new byte[100]);
        var writer = new DiskImageWriter();

        Assert.ThrowsExactly<InvalidDataException>(() => writer.AddFile(zeros, "X", AppleFileType.Text, default));
        Assert.ThrowsExactly<InvalidDataException>(() => writer.AddFile(tiny, "X", AppleFileType.Text, default));
        Assert.ThrowsExactly<ArgumentException>(() => writer.AddFile(new MemoryStream([], false), "X", AppleFileType.Text, default));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetFileContent_BinaryApplesoftAndText_ReturnTheRealLength()
    {
        using var image = OpenImage();
        var writer = new DiskImageWriter();
        byte[] binary = [0x00, 0x03, 0x05, 0x00, 1, 2, 3, 4, 5];
        byte[] basic = [0x03, 0x00, 9, 8, 7];
        writer.AddFile(image, "BIN", AppleFileType.Binary, binary);
        writer.AddFile(image, "PRG", AppleFileType.ApplesoftBasic, basic);
        writer.AddFile(image, "TXT", AppleFileType.Text, "HELLO"u8);
        var commands = new DiskImageCommands();

        CollectionAssert.AreEqual(new byte[] { 1, 2, 3, 4, 5 }, commands.GetFileContent(image, Find(image, "BIN")));
        CollectionAssert.AreEqual(new byte[] { 9, 8, 7 }, commands.GetFileContent(image, Find(image, "PRG")));
        CollectionAssert.AreEqual("HELLO"u8.ToArray(), commands.GetFileContent(image, Find(image, "TXT")));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetFileContent_HeaderClaimsTooMuch_IsClippedToTheSectors()
    {
        using var image = OpenImage();
        new DiskImageWriter().AddFile(image, "LIAR", AppleFileType.Binary, new byte[] { 0, 3, 0xFF, 0xFF, 1, 2 });

        var content = new DiskImageCommands().GetFileContent(image, Find(image, "LIAR"));

        Assert.AreEqual(256 - 4, content.Length);
        Assert.AreEqual(1, content[0]);
    }
}