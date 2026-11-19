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
}
