using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Media.Metadata.Descriptors;
using OoBDev.TestUtilities;
using System.Linq;
using System.Text;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class DescriptorReaderTests
{
    public required TestContext TestContext { get; set; }

    private static byte[] Jpeg(byte marker, byte[] payload)
    {
        var length = payload.Length + 2;
        return [0xFF, 0xD8, 0xFF, marker, (byte)(length >> 8), (byte)length, .. payload, 0xFF, 0xD9];
    }

    private static byte[] Dataset(byte id, string text)
    {
        var b = Encoding.UTF8.GetBytes(text);
        return [0x1C, 2, id, (byte)(b.Length >> 8), (byte)b.Length, .. b];
    }

    [TestMethod]
    public void Iptc_Record2_ReadsFields()
    {
        var iim = new[] { Dataset(5, "Sunset"), Dataset(25, "sea"), Dataset(25, "sky"), Dataset(80, "Ann"), Dataset(120, "Evening"), Dataset(55, "20190504") }.SelectMany(x => x).ToArray();
        byte[] resource = [.. "8BIM"u8.ToArray(), 0x04, 0x04, 0, 0, 0, 0, (byte)(iim.Length >> 8), (byte)iim.Length, .. iim];
        var file = Jpeg(0xED, [.. "Photoshop 3.0\0"u8.ToArray(), .. resource]);

        var d = IptcReader.Read(file);

        TestContext.WriteLine(string.Join("|", d.Title, d.Creator, d.Description, string.Join(",", d.Keywords), d.Created));
        Assert.AreEqual("Sunset", d.Title);
        Assert.AreEqual("Ann", d.Creator);
        Assert.AreEqual("Evening", d.Description);
        CollectionAssert.AreEqual(new[] { "sea", "sky" }, d.Keywords.ToArray());
        Assert.AreEqual("20190504", d.Created);
    }

    [TestMethod]
    public void Xmp_PacketInApp1_ReadsFields()
    {
        const string xml = """
            <x:xmpmeta xmlns:x="adobe:ns:meta/"><rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#">
            <rdf:Description xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:xmp="http://ns.adobe.com/xap/1.0/" xmp:Rating="4" xmp:CreateDate="2019-05-04T10:00:00">
            <dc:title><rdf:Alt><rdf:li xml:lang="x-default">Lake</rdf:li></rdf:Alt></dc:title>
            <dc:creator><rdf:Seq><rdf:li>Bob</rdf:li></rdf:Seq></dc:creator>
            <dc:subject><rdf:Bag><rdf:li>water</rdf:li><rdf:li>trip</rdf:li></rdf:Bag></dc:subject>
            </rdf:Description></rdf:RDF></x:xmpmeta>
            """;
        var file = Jpeg(0xE1, [.. "http://ns.adobe.com/xap/1.0/\0"u8.ToArray(), .. Encoding.UTF8.GetBytes(xml)]);

        var d = XmpReader.Read(file);

        Assert.AreEqual("Lake", d.Title);
        Assert.AreEqual("Bob", d.Creator);
        Assert.AreEqual(4, d.Rating);
        Assert.AreEqual("2019-05-04T10:00:00", d.Created);
        CollectionAssert.AreEqual(new[] { "water", "trip" }, d.Keywords.ToArray());
    }

    [TestMethod]
    public void Readers_BrokenOrMissing_ReturnEmpty()
    {
        Assert.IsTrue(XmpReader.Parse("<not xml").IsEmpty);
        Assert.IsTrue(XmpReader.Read([]).IsEmpty);
        Assert.IsTrue(IptcReader.Read(new byte[10]).IsEmpty);
        Assert.IsTrue(IptcReader.Read(Jpeg(0xED, "Photoshop 3.0\0"u8.ToArray())).IsEmpty);
        Assert.IsTrue(XmpReader.Parse("<!DOCTYPE a [<!ENTITY x \"y\">]><a/>").IsEmpty);
    }

    private const string Packet = """<x:xmpmeta xmlns:x="adobe:ns:meta/"><rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#"><rdf:Description xmlns:xmp="http://ns.adobe.com/xap/1.0/" xmp:Rating="3"/></rdf:RDF></x:xmpmeta>""";

    private static byte[] PngChunk(string type, byte[] data) =>
        [(byte)(data.Length >> 24), (byte)(data.Length >> 16), (byte)(data.Length >> 8), (byte)data.Length, .. Encoding.ASCII.GetBytes(type), .. data, 0, 0, 0, 0];

    [TestMethod]
    public void Xmp_PngItxtChunk_ReadsFields()
    {
        byte[] body = [.. Encoding.ASCII.GetBytes("XML:com.adobe.xmp"), 0, 0, 0, 0, 0, .. Encoding.UTF8.GetBytes(Packet)];
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, .. PngChunk("IHDR", new byte[13]), .. PngChunk("iTXt", body), .. PngChunk("IEND", [])];

        var d = XmpReader.Read(png);

        TestContext.WriteLine("rating {0}", d.Rating);
        Assert.AreEqual(3, d.Rating);
    }

    [TestMethod]
    public void Xmp_WebpXmpChunk_ReadsFieldsAndSkipsOddPadding()
    {
        var xml = Encoding.UTF8.GetBytes(Packet);
        byte[] chunks = [.. "VP8X"u8.ToArray(), 3, 0, 0, 0, 1, 2, 3, 0, .. "XMP "u8.ToArray(), (byte)xml.Length, (byte)(xml.Length >> 8), 0, 0, .. xml];
        byte[] webp = [.. "RIFF"u8.ToArray(), 0, 0, 0, 0, .. "WEBP"u8.ToArray(), .. chunks];

        var d = XmpReader.Read(webp);

        Assert.AreEqual(3, d.Rating);
    }

    [TestMethod]
    public void Xmp_TiffTag700_ReadsFieldsInBothByteOrders()
    {
        var xml = Encoding.UTF8.GetBytes(Packet);
        // IFD at 8: one entry, tag 700 (0x02BC), type 7 (undefined), count, offset 26 (8 + 2 + 12 + 4).
        byte[] le = [.. "II"u8.ToArray(), 42, 0, 8, 0, 0, 0, 1, 0, 0xBC, 0x02, 7, 0, (byte)xml.Length, (byte)(xml.Length >> 8), 0, 0, 26, 0, 0, 0, 0, 0, 0, 0, .. xml];
        byte[] be = [.. "MM"u8.ToArray(), 0, 42, 0, 0, 0, 8, 0, 1, 0x02, 0xBC, 0, 7, 0, 0, (byte)(xml.Length >> 8), (byte)xml.Length, 0, 0, 0, 26, 0, 0, 0, 0, .. xml];

        Assert.AreEqual(3, XmpReader.Read(le).Rating);
        Assert.AreEqual(3, XmpReader.Read(be).Rating);
    }

    [TestMethod]
    public void Xmp_PngCompressedOrTruncated_IsEmpty()
    {
        byte[] compressed = [.. Encoding.ASCII.GetBytes("XML:com.adobe.xmp"), 0, 1, 0, 0, 0, 1, 2, 3];
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, .. PngChunk("iTXt", compressed)];

        Assert.IsTrue(XmpReader.Read(png).IsEmpty);
        Assert.IsTrue(XmpReader.Read(png[..14]).IsEmpty);
    }
}
