using System;

namespace OoBDev.System.IO.Ports;

public static class BridgeExtensions
{
    public static global::System.IO.Ports.Parity AsSystem(this Parity parity) =>
        parity switch
        {
            Parity.None => global::System.IO.Ports.Parity.None,
            Parity.Even => global::System.IO.Ports.Parity.Even,
            Parity.Mark => global::System.IO.Ports.Parity.Mark,
            Parity.Odd => global::System.IO.Ports.Parity.Odd,
            Parity.Space => global::System.IO.Ports.Parity.Space,
            _ => throw new ArgumentException(nameof(parity))
        };
    public static global::System.IO.Ports.StopBits AsSystem(this StopBits stopBits) =>
        stopBits switch
        {
            StopBits.None => global::System.IO.Ports.StopBits.None,
            StopBits.One => global::System.IO.Ports.StopBits.One,
            StopBits.OnePointFive => global::System.IO.Ports.StopBits.OnePointFive,
            StopBits.Two => global::System.IO.Ports.StopBits.Two,
            _ => throw new ArgumentException(nameof(stopBits))
        };
}
