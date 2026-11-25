using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Fencing.ScoreMachines.Favero;
using OoBDev.Fencing.ScoreMachines.SaintGeorge;
using OoBDev.System.IO.Messages;
using OoBDev.System.IO.Ports;
using System;
using System.Buffers;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OoBDev.Fencing.ScoreMachines.Tests;

[TestClass]
public class ScoreMachineDeviceTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void FaveroDevice_Attributes_DeclareSerialSettings()
    {
        var port = typeof(FaveroDevice).GetCustomAttribute<SerialPortAttribute>()!;

        Assert.AreEqual(2400, port.BaudRate);
        Assert.AreEqual(8, port.DataBits);
        Assert.IsNotNull(new FaveroDevice().SegmentDefintion);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SgDevice_Attributes_DeclareSerialSettings()
    {
        var port = typeof(SgDevice).GetCustomAttribute<SerialPortAttribute>()!;

        Assert.AreEqual(9600, port.BaudRate);
        Assert.IsNotNull(new SgDevice().SegmentDefintion);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void FaveroMessageDecoder_Frame_DecodesState()
    {
        var bytes = new byte[] { 0xff, 0x06, 0x12, 0x56, 0x02, 0x14, 0x0a, 0x00, 0x38, 0x56 };

        var state = new FaveroMessageDecoder().Decode(new ReadOnlySequence<byte>(bytes));

        Assert.AreEqual(12, state.Left.Score);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void FaveroMessageDecoder_EmptyFrame_Throws()
    {
        Assert.ThrowsExactly<FormatException>(() => new FaveroMessageDecoder().Decode(ReadOnlySequence<byte>.Empty));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void SgMessageDecoder_Frames_KeepsStateBetweenCalls()
    {
        var decoder = new SgMessageDecoder();
        decoder.Decode(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("\u0001\u0013S01:06\u0004")));

        var state = decoder.Decode(new ReadOnlySequence<byte>(Encoding.ASCII.GetBytes("\u0001\u0013T02:16\u0004")));

        Assert.AreEqual(6, state.Left.Score);
        Assert.AreEqual(new TimeSpan(0, 2, 16), state.Clock);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryAddFencingScoreMachines_Services_RegistersKeyedDecodersOnce()
    {
        var services = new ServiceCollection();

        services.TryAddFencingScoreMachines().TryAddFencingScoreMachines();

        var keys = services.Where(d => d.ServiceType == typeof(IMessageDecoder<IScoreMachineState>)).Select(d => d.ServiceKey).ToList();
        CollectionAssert.AreEquivalent(new object[] { FencingGlobals.FaveroKey, FencingGlobals.SaintGeorgeKey }, keys);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryAddFencingScoreMachines_NullServices_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ServiceCollectionExtensions.TryAddFencingScoreMachines(null!));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ScoreMachineState_Equality_ComparesAllFields()
    {
        var left = new Fencer(5, Cards.Red, Lights.Touch, true);
        var a = new ScoreMachineState(left, default, TimeSpan.FromSeconds(30), 1);
        var b = new ScoreMachineState(left, default, TimeSpan.FromSeconds(30), 1);

        Assert.AreEqual(a, b);
        Assert.AreNotEqual(a, b with { Match = 2 });
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void ToString_State_UsesFixedFormat()
    {
        var state = new ScoreMachineState(new Fencer(5, Cards.Red, Lights.Touch, true), default, new TimeSpan(0, 2, 30), 1);

        Assert.AreEqual("R:S>005 L>Touch C>Red P>True G:S>000 L>None C>None P>False T:00:02:30 M:1", state.ToString());
    }
}
