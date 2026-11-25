using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;

namespace OoBDev.Fencing.ScoreMachines.Panels.Tests;

[TestClass]
[TestCategory("Unit")]
public sealed class FencingPanelTests
{
    private static readonly byte[] _favero = [0xff, 0x06, 0x12, 0x56, 0x02, 0x14, 0x0a, 0x00, 0x38, 0x56];

    private static ReadOnlySequence<byte> Seq(params byte[] bytes) => new(bytes);

    private static OoBDev.UiDefinitions.IControlSurface Surface(OoBDev.UiDefinitions.IDevicePanel panel, OoBDev.UiDefinitions.DeviceSend send) =>
        panel.CreateSurface(send, panel.CreateDecoder());

    [TestMethod]
    public void IsAvailable_Transport_OnlyForSerial()
    {
        Assert.IsTrue(new FaveroPanel().IsAvailable("serial", 0, 0));
        Assert.IsFalse(new SaintGeorgePanel().IsAvailable("tcp", 0, 0));
    }

    [TestMethod]
    public void Panels_PresenterName_MatchesTheDecoderAndTheProviderKey()
    {
        Assert.AreEqual(FencingGlobals.FaveroKey, new FaveroPanel().CreateDecoder().Name);
        Assert.AreEqual(FencingGlobals.SaintGeorgeKey, new SaintGeorgePanel().PresenterName);
        Assert.AreEqual(new SaintGeorgePanel().PresenterName, new SaintGeorgePanel().CreateDecoder().Name);
    }

    [TestMethod]
    public void BuildDefinition_EveryPublishedValue_HasAnIndicator()
    {
        var ids = new FaveroPanel().BuildDefinition().Sections.SelectMany(s => s.Controls).OfType<IndicatorControl>().Select(c => c.Id).ToList();
        var decoder = new ScoreMachineDecoder(MachineType.Favero);
        IReadOnlyDictionary<string, string>? published = null;
        decoder.ValuesChanged += (_, values) => published = values;

        decoder.Decode(Seq(_favero));

        Assert.IsNotNull(published);
        CollectionAssert.IsSubsetOf(published.Keys.ToList(), ids);
    }

    [TestMethod]
    public async Task InvokeAsync_AnyCommand_Throws()
    {
        var surface = Surface(new FaveroPanel(), (_, _) => Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync("reset", null, TestContext.CancellationToken));
    }

    [TestMethod]
    public void Decode_FaveroFrameSplitAcrossChunksWithNoise_YieldsOneLine()
    {
        var decoder = new ScoreMachineDecoder(MachineType.Favero);
        var published = new List<IReadOnlyDictionary<string, string>>();
        decoder.ValuesChanged += (_, values) => published.Add(values);

        var first = decoder.Decode(Seq([0x00, 0x13, .. _favero[..4]]));
        var second = decoder.Decode(Seq(_favero[4..]));

        Assert.IsEmpty(first);
        Assert.HasCount(1, second);
        StringAssert.Contains(second[0], "T:00:02:56");
        Assert.HasCount(1, published);
        Assert.AreEqual("12", published[0]["leftScore"]);
        Assert.AreEqual("6", published[0]["rightScore"]);
        Assert.AreEqual("02:56", published[0]["clock"]);
        Assert.AreEqual("yes", published[0]["leftPriority"]);
    }

    [TestMethod]
    public void Decode_RepeatedFrame_PublishesOnlyTheFirstTime()
    {
        var decoder = new ScoreMachineDecoder(MachineType.Favero);
        var published = 0;
        decoder.ValuesChanged += (_, _) => published++;

        var lines = decoder.Decode(Seq([.. _favero, .. _favero]));

        Assert.HasCount(2, lines);
        Assert.AreEqual(1, published);
    }

    [TestMethod]
    public void Decode_SaintGeorgeScoreFrame_UpdatesBothScores()
    {
        var decoder = new ScoreMachineDecoder(MachineType.Sg);
        IReadOnlyDictionary<string, string>? published = null;
        decoder.ValuesChanged += (_, values) => published = values;
        var frame = Encoding.ASCII.GetBytes("junk\u0001\u0013ST\u0002011:005\u000202\u000201\u0004");

        var lines = decoder.Decode(Seq(frame));

        Assert.HasCount(1, lines);
        Assert.IsNotNull(published);
        Assert.AreEqual("11", published["rightScore"]);
        Assert.AreEqual("5", published["leftScore"]);
    }

    [TestMethod]
    public void Decode_SaintGeorgeFrameOverTheLimit_IsDroppedAndTheNextOneDecodes()
    {
        var decoder = new ScoreMachineDecoder(MachineType.Sg);
        var runaway = new byte[] { 0x01 }.Concat(Enumerable.Repeat((byte)'x', 150)).ToArray();
        var good = Encoding.ASCII.GetBytes("\u0001\u0013ST\u0002011:005\u000202\u000201\u0004");

        var lines = decoder.Decode(Seq([.. runaway, .. good]));

        Assert.HasCount(1, lines);
    }

    [TestMethod]
    public void Decode_MalformedSaintGeorgeScoreFrame_IsReportedNotThrown()
    {
        var decoder = new ScoreMachineDecoder(MachineType.Sg);

        var lines = decoder.Decode(Seq(0x01, 0x13, (byte)'S', (byte)'T', 0x02, (byte)'1', 0x04));

        Assert.HasCount(1, lines);
        StringAssert.StartsWith(lines[0], "BAD FRAME: ");
    }

    public required TestContext TestContext { get; set; }
}
