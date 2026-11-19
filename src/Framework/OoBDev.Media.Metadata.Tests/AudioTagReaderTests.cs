using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Media.Metadata.Audio;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class AudioTagReaderTests
{
    public required TestContext TestContext { get; set; }

    private static byte[] Frame(string id, byte version, params byte[][] body)
    {
        var data = body.SelectMany(b => b).ToArray();
        var size = version == 4
            ? new[] { (byte)((data.Length >> 21) & 0x7F), (byte)((data.Length >> 14) & 0x7F), (byte)((data.Length >> 7) & 0x7F), (byte)(data.Length & 0x7F) }
            : new[] { (byte)(data.Length >> 24), (byte)(data.Length >> 16), (byte)(data.Length >> 8), (byte)data.Length };
        return [.. Encoding.ASCII.GetBytes(id), .. size, 0, 0, .. data];
    }

    private static byte[] Text(string id, byte version, string text, byte encoding = 3)
        => Frame(id, version, [encoding], Encoding.UTF8.GetBytes(text));

    private static byte[] Id3v2(byte version, params byte[][] frames)
    {
        var body = frames.SelectMany(f => f).ToArray();
        var size = new[] { (byte)((body.Length >> 21) & 0x7F), (byte)((body.Length >> 14) & 0x7F), (byte)((body.Length >> 7) & 0x7F), (byte)(body.Length & 0x7F) };
        return [.. "ID3"u8.ToArray(), version, 0, 0, .. size, .. body];
    }

    [TestMethod]
    [DataRow((byte)3)]
    [DataRow((byte)4)]
    public void Read_Id3v2TextFrames_ReturnsNormalizedKeys(byte version)
    {
        var file = Id3v2(version, Text("TIT2", version, "Song"), Text("TPE1", version, "Band"), Text("TALB", version, "Record"), Text("TRCK", version, "3/12"), Text("TCON", version, "(17)"));

        var tags = AudioTagReader.Read(file);

        TestContext.WriteLine($"format={tags.Format} values={string.Join(",", tags.Values.Select(v => v.Key + "=" + v.Value))}");
        Assert.AreEqual("id3v2", tags.Format);
        Assert.AreEqual("Song", tags.Values["title"]);
        Assert.AreEqual("Band", tags.Values["artist"]);
        Assert.AreEqual("Record", tags.Values["album"]);
        Assert.AreEqual("3/12", tags.Values["track"]);
        Assert.AreEqual("Rock", tags.Values["genre"]);
    }

    [TestMethod]
    public void Read_Id3v2Utf16WithBom_Decodes()
    {
        var utf16 = new byte[] { 1, 0xFF, 0xFE }.Concat(Encoding.Unicode.GetBytes("Grüße")).ToArray();
        var file = Id3v2(3, Frame("TIT2", 3, utf16));

        Assert.AreEqual("Grüße", AudioTagReader.Read(file).Values["title"]);
    }

    [TestMethod]
    public void Read_Id3v2Comment_SkipsLanguageAndDescription()
    {
        var body = new byte[] { 3 }.Concat("eng"u8.ToArray()).Concat(new byte[] { (byte)'d', 0 }).Concat("hello"u8.ToArray()).ToArray();
        var file = Id3v2(3, Frame("COMM", 3, body));

        Assert.AreEqual("hello", AudioTagReader.Read(file).Values["comment"]);
    }

    [TestMethod]
    public void Read_Id3v2FrameLongerThanTag_StopsWithoutException()
    {
        var file = Id3v2(3, Text("TIT2", 3, "Ok"));
        var broken = file.Concat(new byte[] { (byte)'T', (byte)'P', (byte)'E', (byte)'1', 0x7F, 0, 0, 0, 0, 0 }).ToArray();

        var tags = AudioTagReader.Read(broken);

        Assert.AreEqual("Ok", tags.Values["title"]);
    }

    [TestMethod]
    public void Read_Id3v1_ReadsFixedFields()
    {
        var tag = new byte[128];
        "TAG"u8.CopyTo(tag);
        "Old Song"u8.CopyTo(tag.AsSpan(3));
        "Old Band"u8.CopyTo(tag.AsSpan(33));
        "1999"u8.CopyTo(tag.AsSpan(93));
        tag[125] = 0;
        tag[126] = 7;
        tag[127] = 13;
        var file = new byte[1000].Concat(tag).ToArray();

        var tags = AudioTagReader.Read(file);

        Assert.AreEqual("id3v1", tags.Format);
        Assert.AreEqual("Old Song", tags.Values["title"]);
        Assert.AreEqual("1999", tags.Values["date"]);
        Assert.AreEqual("7", tags.Values["track"]);
        Assert.AreEqual("Pop", tags.Values["genre"]);
    }

    [TestMethod]
    public void Read_Flac_ReadsVorbisComments()
    {
        static byte[] Le(int v) => BitConverter.GetBytes(v);
        var comments = new List<string> { "title=Free", "ARTIST=Open", "ignored=x" };
        var block = Le(4).Concat("vend"u8.ToArray()).Concat(Le(comments.Count)).Concat(comments.SelectMany(c => Le(c.Length).Concat(Encoding.UTF8.GetBytes(c)))).ToArray();
        var header = new byte[] { 0x84, (byte)(block.Length >> 16), (byte)(block.Length >> 8), (byte)block.Length };
        var file = "fLaC"u8.ToArray().Concat(header).Concat(block).ToArray();

        var tags = AudioTagReader.Read(file);

        Assert.AreEqual("vorbis", tags.Format);
        Assert.AreEqual("Free", tags.Values["title"]);
        Assert.AreEqual("Open", tags.Values["artist"]);
        Assert.AreEqual(2, tags.Values.Count);
    }

    private static byte[] Box(string type, params byte[][] body)
    {
        var data = body.SelectMany(b => b).ToArray();
        var size = data.Length + 8;
        return [(byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size, .. Encoding.Latin1.GetBytes(type), .. data];
    }

    [TestMethod]
    public void Read_Mp4_ReadsIlstItems()
    {
        var title = Box("©nam", Box("data", [0, 0, 0, 1, 0, 0, 0, 0], Encoding.UTF8.GetBytes("Tune")));
        var track = Box("trkn", Box("data", [0, 0, 0, 0, 0, 0, 0, 0], [0, 0, 0, 5, 0, 9, 0, 0]));
        var ilst = Box("ilst", title, track);
        var meta = Box("meta", [0, 0, 0, 0], ilst);
        var file = Box("ftyp", "M4A "u8.ToArray(), [0, 0, 0, 0]).Concat(Box("moov", Box("udta", meta))).ToArray();

        var tags = AudioTagReader.Read(file);

        TestContext.WriteLine($"format={tags.Format} values={string.Join(",", tags.Values.Select(v => v.Key + "=" + v.Value))}");
        Assert.AreEqual("mp4", tags.Format);
        Assert.AreEqual("Tune", tags.Values["title"]);
        Assert.AreEqual("5/9", tags.Values["track"]);
    }

    [TestMethod]
    public void Read_Mp4WithoutIlst_ReturnsEmpty()
    {
        var file = Box("ftyp", "M4A "u8.ToArray(), [0, 0, 0, 0]).Concat(Box("moov", Box("trak"))).ToArray();

        Assert.AreEqual(0, AudioTagReader.Read(file).Values.Count);
    }

    [TestMethod]
    public void Read_NoTags_ReturnsEmpty()
    {
        Assert.AreEqual(0, AudioTagReader.Read(new byte[300]).Values.Count);
        Assert.AreEqual(0, AudioTagReader.Read([]).Values.Count);
        Assert.AreEqual(0, AudioTagReader.Read("fLaC"u8.ToArray()).Values.Count);
    }
}
