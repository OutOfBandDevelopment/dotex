using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Devices.TestUtilities;
using OoBDev.System.IO.Messages;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Velleman.K8055.Tests;

[TestClass]
public class K8055Tests
{
    // report id 00, digital 00, 03, A1 76, A2 76, counter1 0x012A = 298, counter2 0 (dev-term, confirmed on a real board)
    private static readonly byte[] SampleReport = [0x00, 0x00, 0x03, 0x4C, 0x4C, 0x2A, 0x01, 0x00, 0x00];

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_ConfirmedReport_ReadsAnalogueAndCounters()
    {
        var reading = new K8055MessageDecoder().Decode(new ReadOnlySequence<byte>(SampleReport));

        Assert.AreEqual(new K8055Reading(0x00, 76, 76, 298, 0), reading);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void DigitalInputs_RawByte_NamesTheBits()
    {
        var report = (byte[])SampleReport.Clone();
        report[1] = 0x11;

        var reading = new K8055MessageDecoder().Decode(new ReadOnlySequence<byte>(report));

        Assert.AreEqual(K8055DigitalInputs.I3 | K8055DigitalInputs.I1, reading.DigitalInputs);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_WrongLength_Throws() =>
        Assert.ThrowsExactly<ArgumentException>(() => new K8055MessageDecoder().Decode(new ReadOnlySequence<byte>(SampleReport.AsMemory(0, 8))));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Encode_SetOutputs_CarriesDigitalMaskAndBothAnalogueBytes()
    {
        var command = K8055Command.SetOutputs(new K8055Outputs(0b0000_0101, 128, 255));

        var bytes = new K8055MessageEncoder().Encode(ref command).ToArray();

        CollectionAssert.AreEqual(new byte[] { 0x00, 0x05, 0x05, 0x80, 0xFF, 0x00, 0x00, 0x00, 0x00 }, bytes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(true, 0x03)]
    [DataRow(false, 0x04)]
    public void Encode_ResetCounter_IsAFixedFrame(bool first, int command)
    {
        var request = first ? K8055Command.ResetCounter1 : K8055Command.ResetCounter2;

        var bytes = new K8055MessageEncoder().Encode(ref request).ToArray();

        CollectionAssert.AreEqual(new byte[] { 0x00, (byte)command, 0, 0, 0, 0, 0, 0, 0 }, bytes);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WithDigital_SwitchesOneChannelAndKeepsTheRest()
    {
        var outputs = new K8055Outputs(0, 10, 20).WithDigital(1, true).WithDigital(3, true).WithDigital(1, false);

        Assert.AreEqual(new K8055Outputs(0b0000_0100, 10, 20), outputs);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(0)]
    [DataRow(9)]
    public void WithDigital_ChannelOutOfRange_Throws(int channel) =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new K8055Outputs().WithDigital(channel, true));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Push_PartialThenRestOfAReportAndASecond_YieldsTwoReadings()
    {
        var pump = new SegmentPump<K8055Reading>(new K8055Device());

        await pump.PushAsync(SampleReport.AsMemory(0, 5)).ConfigureAwait(false);
        Assert.AreEqual(0, pump.Messages.Count);
        await pump.PushAsync(SampleReport.Skip(5).Concat(SampleReport).ToArray()).ConfigureAwait(false);

        Assert.AreEqual(2, pump.Messages.Count);
        Assert.AreEqual((ushort)298, pump.Messages[1].Counter1);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddK8055_RegistersEncoderAndDecoder()
    {
        using var provider = new ServiceCollection().TryAddK8055().BuildServiceProvider();

        Assert.IsInstanceOfType<K8055MessageDecoder>(provider.GetRequiredService<IMessageDecoder<K8055Reading>>());
        Assert.IsInstanceOfType<K8055MessageEncoder>(provider.GetRequiredService<IMessageEncoder<K8055Command>>());
    }
}
