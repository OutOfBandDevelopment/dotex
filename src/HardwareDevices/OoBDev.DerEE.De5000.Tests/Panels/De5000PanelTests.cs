using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DerEE.De5000.Tests;
using OoBDev.TestUtilities;
using OoBDev.UiDefinitions;

namespace OoBDev.DerEE.De5000.Panels.Tests;

[TestClass]
[TestCategory(TestCategories.DerEe_De5000)]
public sealed class De5000PanelTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("serial", true)]
    [DataRow("BLE", true)]
    [DataRow("tcp", false)]
    public void IsAvailable_Transport_SerialAndBleOnly(string transport, bool expected) =>
        Assert.AreEqual(expected, new De5000Panel().IsAvailable(transport, 0, 0));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void BuildDefinition_EveryInterpreterValue_HasAnIndicator()
    {
        var ids = new De5000Panel().BuildDefinition().Sections.SelectMany(s => s.Controls).OfType<IndicatorControl>().Select(c => c.Id).ToHashSet();
        var frame = De5000Frame.TryParse(De5000FrameTests.Valid, out var parsed) ? parsed : throw new InvalidOperationException();

        var values = De5000Interpreter.Interpret(frame).Values.Keys;

        CollectionAssert.IsSubsetOf(values.ToList(), ids.ToList());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task InvokeAsync_AnyCommand_Throws()
    {
        var surface = new De5000Panel().CreateSurface((_, _) => Task.CompletedTask);

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => surface.InvokeAsync("reset", null, TestContext.CancellationToken));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_SplitPacketWithNoise_PublishesThePrimaryReading()
    {
        var decoder = new De5000Decoder();
        var published = new List<IReadOnlyDictionary<string, string>>();
        decoder.ValuesChanged += (_, values) => published.Add(values);
        var bytes = new byte[] { 0x55, 0x01 }.Concat(De5000FrameTests.Valid).ToArray();

        var first = decoder.Decode(new ReadOnlySequence<byte>(bytes.AsMemory(0, 9)));
        var second = decoder.Decode(new ReadOnlySequence<byte>(bytes.AsMemory(9)));

        Assert.IsEmpty(first);
        Assert.HasCount(1, second);
        Assert.StartsWith("Ls=1.234mH", second[0]);
        Assert.AreEqual("Ls=1.234mH", published.Single()["primary"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_SamePacketTwice_PublishesOnlyOnce()
    {
        var decoder = new De5000Decoder();
        var count = 0;
        decoder.ValuesChanged += (_, _) => count++;

        var lines = decoder.Decode(new ReadOnlySequence<byte>(De5000FrameTests.Valid.Concat(De5000FrameTests.Valid).ToArray()));

        Assert.HasCount(2, lines);
        Assert.AreEqual(1, count);
    }

    public required TestContext TestContext { get; set; }
}
