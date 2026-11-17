using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Archives.Tar;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace OoBDev.System.Tests.Archives.Tar;

[TestClass]
public class TarUtilitiesTests
{
    private static byte[] Block(string name, string size, char type, string modified = "14713153200")
    {
        var block = new byte[OoBDev.System.Archives.Tar.Utilities.HeaderBlockSize];
        Encoding.ASCII.GetBytes(name).CopyTo(block, 0);
        Encoding.ASCII.GetBytes("0000644").CopyTo(block, 100);
        Encoding.ASCII.GetBytes(size).CopyTo(block, 124);
        Encoding.ASCII.GetBytes(modified).CopyTo(block, 136);
        block[156] = (byte)type;
        Encoding.ASCII.GetBytes("target.txt").CopyTo(block, 157);
        return block;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToHeader_UstarBlock_ParsesFields()
    {
        var header = Block("dir/hello.txt", "00000000017 ", '0').ToHeader();

        Assert.AreEqual("dir/hello.txt", header.FileName);
        Assert.AreEqual("0000644", header.FileMode);
        Assert.AreEqual(15, header.FileSize);
        Assert.AreEqual(Convert.ToInt32("14713153200", 8), header.LastModifiedTime);
        Assert.AreEqual(TarFileType.File, header.FileType);
        Assert.AreEqual("target.txt", header.LinkedFile);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToHeader_BlankBlock_HasEmptyNameAndZeroSize()
    {
        var header = new byte[OoBDev.System.Archives.Tar.Utilities.HeaderBlockSize].ToHeader();

        Assert.AreEqual(string.Empty, header.FileName);
        Assert.AreEqual(0, header.FileSize);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToHeader_ShortBlock_Throws() =>
        Assert.ThrowsExactly<ArgumentException>(() => new byte[100].ToHeader());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToHeader_NonOctalSize_Throws() =>
        Assert.ThrowsExactly<InvalidDataException>(() => Block("a", "12x", '0').ToHeader());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToHeader_SizeBeyondInt32_Throws() =>
        Assert.ThrowsExactly<InvalidDataException>(() => Block("a", "77777777777", '0').ToHeader());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToString_Field_TrimsPadding()
    {
        var block = Block("abc", "0", '0');

        Assert.AreEqual("abc", block.ToString(0, 100));
        Assert.IsNull(block.ToString(200, 10));
        Assert.IsNull(Array.Empty<byte>().ToString(0, 1));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decompress_GzipBytesAndStream_ReturnOriginal()
    {
        var original = Encoding.ASCII.GetBytes("tar payload tar payload tar payload");
        using var compressed = new MemoryStream();
        using (var gz = new GZipStream(compressed, CompressionMode.Compress, true))
        {
            gz.Write(original);
        }

        CollectionAssert.AreEqual(original, OoBDev.System.Archives.Tar.Utilities.Decompress(compressed.ToArray()));
        Assert.IsNull(OoBDev.System.Archives.Tar.Utilities.Decompress(Array.Empty<byte>()));
        compressed.Position = 0;
        using var reader = new MemoryStream();
        OoBDev.System.Archives.Tar.Utilities.Decompress(compressed).CopyTo(reader);
        CollectionAssert.AreEqual(original, reader.ToArray());
    }
}
