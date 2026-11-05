using System.IO.Ports;
using OoBDev.System.IO.Ports.Bridge.Tests.TestKit;
using static OoBDev.System.IO.Ports.Bridge.Rfc2217Codes;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
[TestCategory(TestCategories.Rfc2217)]
public sealed class Rfc2217SessionTests
{
    private static byte[] Sub(byte command, params byte[] data) =>
        [Iac, Sb, OptionComPort, command, .. data, Iac, Se];

    [TestMethod]
    public void PlainBytesPassThroughAndEscapedIacIsUnescaped()
    {
        var session = new Rfc2217Session(new BridgeOptions(), () => null);

        var result = session.Process([1, 2, Iac, Iac, 3]);

        CollectionAssert.AreEqual(new byte[] { 1, 2, Iac, 3 }, result.SerialData);
        Assert.IsEmpty(result.Reply);
    }

    [TestMethod]
    public void EscapeDoublesIacBytes()
    {
        var escaped = Rfc2217Session.Escape(new byte[] { 1, Iac, 2 });

        CollectionAssert.AreEqual(new byte[] { 1, Iac, Iac, 2 }, escaped.ToArray());
    }

    [TestMethod]
    public void GreetingOffersComPortOption()
    {
        var greeting = new Rfc2217Session(new BridgeOptions(), () => null).GetGreeting();

        CollectionAssert.AreEqual(new byte[] { Iac, Will, OptionComPort }, greeting[..3]);
    }

    [TestMethod]
    public void AcceptedOfferGetsNoReplyButUnknownOptionIsRefused()
    {
        var session = new Rfc2217Session(new BridgeOptions(), () => null);
        session.GetGreeting();

        var accepted = session.Process([Iac, Do, OptionComPort]);
        var refused = session.Process([Iac, Do, 31]);

        Assert.IsEmpty(accepted.Reply);
        CollectionAssert.AreEqual(new byte[] { Iac, Wont, 31 }, refused.Reply);
    }

    [TestMethod]
    public void SetBaudRateChangesPortAndConfirms()
    {
        var link = new FakeSerialLink();
        var session = new Rfc2217Session(new BridgeOptions(), () => link);

        var result = session.Process(Sub(SetBaudRate, 0, 0, 0x25, 0x80)); // 9600

        Assert.AreEqual(9600, link.BaudRate);
        CollectionAssert.AreEqual(Sub((byte)(SetBaudRate + ServerOffset), 0, 0, 0x25, 0x80), result.Reply);
    }

    [TestMethod]
    public void SetDataParityStopAreApplied()
    {
        var link = new FakeSerialLink();
        var session = new Rfc2217Session(new BridgeOptions(), () => link);

        session.Process(Sub(SetDataSize, 7));
        session.Process(Sub(SetParity, 3));
        session.Process(Sub(SetStopSize, 2));

        Assert.AreEqual(7, link.DataBits);
        Assert.AreEqual(global::System.IO.Ports.Parity.Even, link.Parity);
        Assert.AreEqual(global::System.IO.Ports.StopBits.Two, link.StopBits);
    }

    [TestMethod]
    public void ControlCommandsDriveDtrRtsAndBreak()
    {
        var link = new FakeSerialLink();
        var session = new Rfc2217Session(new BridgeOptions(), () => link);

        session.Process(Sub(SetControl, 8));
        session.Process(Sub(SetControl, 11));
        session.Process(Sub(SetControl, 5));

        Assert.IsTrue(link.DtrEnable);
        Assert.IsTrue(link.RtsEnable);
        Assert.IsTrue(link.BreakState);

        session.Process(Sub(SetControl, 9));
        Assert.IsFalse(link.DtrEnable);
    }

    [TestMethod]
    public void PurgeDiscardsBuffers()
    {
        var link = new FakeSerialLink();
        var session = new Rfc2217Session(new BridgeOptions(), () => link);

        session.Process(Sub(PurgeData, 3));

        Assert.AreEqual(1, link.InPurges);
        Assert.AreEqual(1, link.OutPurges);
    }

    [TestMethod]
    public void SubnegotiationSplitAcrossReadsIsReassembled()
    {
        var link = new FakeSerialLink();
        var session = new Rfc2217Session(new BridgeOptions(), () => link);
        var frame = Sub(SetBaudRate, 0, 0, 0x25, 0x80);

        var first = session.Process(frame.AsSpan(0, 6));
        var second = session.Process(frame.AsSpan(6));

        Assert.IsEmpty(first.Reply);
        Assert.IsNotEmpty(second.Reply);
        Assert.AreEqual(9600, link.BaudRate);
    }

    [TestMethod]
    public void ModemStateReportedOnMaskThenOnlyOnChange()
    {
        var link = new FakeSerialLink { CtsHolding = true };
        var session = new Rfc2217Session(new BridgeOptions(), () => link);

        session.Process(Sub(SetModemStateMask, 0xFB));
        var initial = session.PollModemState();
        var unchanged = session.PollModemState();
        link.CtsHolding = false;
        var changed = session.PollModemState();

        Assert.IsNotNull(initial);
        Assert.IsNull(unchanged);
        Assert.IsNotNull(changed);
        Assert.AreEqual(0x01, changed[^3] & 0x11); // delta-CTS set, CTS now low
    }

    [TestMethod]
    public void ResetRestoresConfiguredLineSettings()
    {
        var link = new FakeSerialLink();
        var session = new Rfc2217Session(new BridgeOptions { BaudRate = 57600 }, () => link);
        session.Process(Sub(SetBaudRate, 0, 0, 0x25, 0x80));

        session.Reset();

        Assert.AreEqual(57600, link.BaudRate);
    }
}
