using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO.Fat;
using OoBDev.TestUtilities;
using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

namespace OoBDev.System.Tests.IO.Fat;

[TestClass]
public class FatVolumeTests
{
    private static byte[] Pattern(int length) => [.. Enumerable.Range(0, length).Select(i => (byte)((i * 13) + 5))];

    private static byte[] ReadAll(FatVolume volume, FatDirectoryEntry entry)
    {
        using var stream = volume.OpenRead(entry);
        using var copy = new MemoryStream();
        stream.CopyTo(copy);
        return copy.ToArray();
    }

    private static FatVolume Open(byte[] image) => FatVolume.Open(new MemoryStream(image));

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(FatType.Fat12)]
    [DataRow(FatType.Fat16)]
    [DataRow(FatType.Fat32)]
    public void Open_EachFatType_DetectsTypeListsRootAndReadsFiles(FatType type)
    {
        var builder = new FatImageBuilder(type) { Label = "TESTDISK" };
        var small = Pattern(100);
        var large = Pattern((builder.BytesPerCluster * 3) + 77);
        builder.AddFile(null, "SMALL   TXT", small);
        builder.AddFile(null, "LARGE   BIN", large);
        builder.AddFile(null, "EMPTY   DAT", []);

        var volume = Open(builder.Build());

        Assert.AreEqual(type, volume.Type);
        Assert.AreEqual("TESTDISK", volume.Label);
        var entries = volume.GetEntries();
        CollectionAssert.AreEqual(new[] { "SMALL.TXT", "LARGE.BIN", "EMPTY.DAT" }, entries.Select(e => e.Name).ToArray());
        Assert.AreEqual(3, entries.Count);
        CollectionAssert.AreEqual(small, ReadAll(volume, entries[0]));
        CollectionAssert.AreEqual(large, ReadAll(volume, entries[1]));
        Assert.AreEqual(0, ReadAll(volume, entries[2]).Length);
        Assert.AreEqual(new DateTime(2024, 3, 15, 13, 45, 30), entries[0].Modified);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetEntries_LongNames_AreJoinedAndShortNamesKept()
    {
        var builder = new FatImageBuilder(FatType.Fat16);
        builder.AddFile(null, "LONGNA~1TXT", Pattern(10), "A rather long file name.txt");
        builder.AddFile(null, "PLAIN   TXT", Pattern(10));

        var entries = Open(builder.Build()).GetEntries();

        Assert.AreEqual("A rather long file name.txt", entries[0].Name);
        Assert.AreEqual("LONGNA~1.TXT", entries[0].ShortName);
        Assert.AreEqual("PLAIN.TXT", entries[1].Name);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetEntries_DeletedEntriesAndTheirLongNames_AreSkipped()
    {
        var builder = new FatImageBuilder(FatType.Fat12);
        builder.AddFile(null, "GONE    TXT", Pattern(10), "gone with a long name.txt", deleted: true);
        builder.AddFile(null, "KEPT    TXT", Pattern(10));

        var entries = Open(builder.Build()).GetEntries();

        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("KEPT.TXT", entries[0].Name);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(FatType.Fat12)]
    [DataRow(FatType.Fat32)]
    public void Find_SubdirectoryPath_ReturnsFileCaseInsensitively(FatType type)
    {
        var builder = new FatImageBuilder(type);
        builder.AddDirectory("DOCS       ", "My Docs");
        var data = Pattern(900);
        builder.AddFile("DOCS       ", "NOTES   TXT", data, "notes.txt");
        var volume = Open(builder.Build());

        var directory = volume.Find("my docs");
        var file = volume.Find("MY DOCS\\NOTES.TXT");

        Assert.IsTrue(directory!.IsDirectory);
        Assert.AreEqual("notes.txt", volume.GetEntries(directory).Single().Name);
        CollectionAssert.AreEqual(data, ReadAll(volume, file!));
        Assert.IsNull(volume.Find("docs/missing.txt"));
        Assert.IsNull(volume.Find("docs/notes.txt/deeper"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetEntries_DirectorySpanningClusters_ListsEveryEntry()
    {
        var builder = new FatImageBuilder(FatType.Fat32);
        for (var i = 0; i < 40; i++)
        {
            builder.AddFile(null, $"F{i:D7}TXT", [(byte)i]);
        }

        var volume = Open(builder.Build());

        var entries = volume.GetEntries();
        Assert.AreEqual(40, entries.Count);
        Assert.AreEqual(39, ReadAll(volume, entries[39])[0]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetEntries_LoopingDirectoryChain_ThrowsInvalidData()
    {
        var builder = new FatImageBuilder(FatType.Fat16);
        builder.AddDirectory("LOOP       ");
        var image = builder.Build();
        var cluster = builder.FirstCluster("LOOP       ");
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(builder.FatOffset + (cluster * 2)), (ushort)cluster);
        var volume = Open(image);

        Assert.ThrowsExactly<InvalidDataException>(() => volume.GetEntries(volume.GetEntries().Single()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void OpenRead_ChainEndingBeforeSize_ThrowsInvalidData()
    {
        var builder = new FatImageBuilder(FatType.Fat16);
        builder.AddFile(null, "SHORT   BIN", Pattern(builder.BytesPerCluster * 3));
        var image = builder.Build();
        var cluster = builder.FirstCluster("SHORT   BIN");
        BinaryPrimitives.WriteUInt16LittleEndian(image.AsSpan(builder.FatOffset + (cluster * 2)), 0xFFFF);
        var volume = Open(image);

        Assert.ThrowsExactly<InvalidDataException>(() => volume.OpenRead(volume.GetEntries().Single()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void OpenRead_SizeLargerThanVolume_ThrowsInvalidData()
    {
        var builder = new FatImageBuilder(FatType.Fat12);
        builder.AddFile(null, "HUGE    BIN", Pattern(10));
        var image = builder.Build();
        var rootOffset = (1 + (2 * 9)) * 512;
        BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(rootOffset + 28), 0xF0000000);
        var volume = Open(image);

        Assert.ThrowsExactly<InvalidDataException>(() => volume.OpenRead(volume.GetEntries().Single()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Open_BadImages_ThrowInvalidData()
    {
        var good = new FatImageBuilder(FatType.Fat12).Build();
        var noSignature = (byte[])good.Clone();
        noSignature[510] = 0;
        var truncated = good[..(good.Length / 2)];

        Assert.ThrowsExactly<InvalidDataException>(() => Open(noSignature));
        Assert.ThrowsExactly<InvalidDataException>(() => Open(truncated));
        Assert.ThrowsExactly<InvalidDataException>(() => Open(new byte[512]));
        Assert.ThrowsExactly<InvalidDataException>(() => Open(new byte[10]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetEntries_FileEntry_ThrowsArgument()
    {
        var builder = new FatImageBuilder(FatType.Fat12);
        builder.AddFile(null, "A       TXT", Pattern(1));
        var volume = Open(builder.Build());
        var file = volume.GetEntries().Single();

        Assert.ThrowsExactly<ArgumentException>(() => volume.GetEntries(file));
        Assert.AreEqual(Encoding.ASCII.GetString("A.TXT"u8), file.Name);
    }
}
