using System.IO.Ports;
using static OoBDev.System.IO.Ports.Bridge.Rfc2217Codes;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>
/// Server side of one RFC 2217 client connection: telnet option negotiation plus the COM-PORT-OPTION
/// sub-negotiations (baud rate, data size, parity, stop bits, DTR/RTS/break/flow control, purge,
/// modem-state notifications). Pure byte-in/byte-out so it can be tested without sockets.
/// </summary>
public sealed class Rfc2217Session
{
    private const int MaxSubnegotiationLength = 512;
    private const string SignatureText = "ser2net-win";

    private const byte ModemDeltaCts = 0x01;
    private const byte ModemDeltaDsr = 0x02;
    private const byte ModemDeltaCd = 0x08;
    private const byte ModemCts = 0x10;
    private const byte ModemDsr = 0x20;
    private const byte ModemCd = 0x80;

    private readonly BridgeOptions _options;
    private readonly Func<ISerialControl?> _control;
    private readonly List<byte> _subnegotiation = [];
    private readonly HashSet<byte> _localOn = [];
    private readonly HashSet<byte> _localAsked = [];
    private readonly HashSet<byte> _remoteOn = [];
    private readonly HashSet<byte> _remoteAsked = [];

    private ParseState _state = ParseState.Data;
    private byte _verb;
    private byte _modemMask;
    private byte? _lastModem;

    public Rfc2217Session(BridgeOptions options, Func<ISerialControl?> control)
    {
        _options = options;
        _control = control;
    }

    private enum ParseState
    {
        Data,
        Iac,
        Verb,
        Subnegotiation,
        SubnegotiationIac,
    }

    /// <summary>Doubles 0xFF bytes so serial data survives telnet framing. Returns the input when nothing needs escaping.</summary>
    public static ReadOnlyMemory<byte> Escape(ReadOnlyMemory<byte> data)
    {
        var span = data.Span;
        if (!span.Contains(Iac))
        {
            return data;
        }

        var result = new List<byte>(span.Length + 4);
        foreach (var b in span)
        {
            result.Add(b);
            if (b == Iac)
            {
                result.Add(Iac);
            }
        }

        return result.ToArray();
    }

    /// <summary>The negotiation bytes to send as soon as the client connects.</summary>
    public byte[] GetGreeting()
    {
        var reply = new List<byte>();
        Ask(reply, Will, OptionComPort);
        Ask(reply, Will, OptionBinary);
        Ask(reply, Do, OptionBinary);
        Ask(reply, Will, OptionSuppressGoAhead);
        Ask(reply, Do, OptionSuppressGoAhead);
        return reply.ToArray();
    }

    public Rfc2217Result Process(ReadOnlySpan<byte> input)
    {
        var data = new List<byte>(input.Length);
        var reply = new List<byte>();

        foreach (var b in input)
        {
            switch (_state)
            {
                case ParseState.Data:
                    if (b == Iac)
                    {
                        _state = ParseState.Iac;
                    }
                    else
                    {
                        data.Add(b);
                    }

                    break;

                case ParseState.Iac:
                    if (b == Iac)
                    {
                        data.Add(Iac);
                        _state = ParseState.Data;
                    }
                    else if (b is Will or Wont or Do or Dont)
                    {
                        _verb = b;
                        _state = ParseState.Verb;
                    }
                    else if (b == Sb)
                    {
                        _subnegotiation.Clear();
                        _state = ParseState.Subnegotiation;
                    }
                    else
                    {
                        _state = ParseState.Data; // NOP, GA, etc.
                    }

                    break;

                case ParseState.Verb:
                    Negotiate(_verb, b, reply);
                    _state = ParseState.Data;
                    break;

                case ParseState.Subnegotiation:
                    if (b == Iac)
                    {
                        _state = ParseState.SubnegotiationIac;
                    }
                    else if (_subnegotiation.Count < MaxSubnegotiationLength)
                    {
                        _subnegotiation.Add(b);
                    }

                    break;

                case ParseState.SubnegotiationIac:
                    if (b == Se)
                    {
                        HandleSubnegotiation(reply);
                        _state = ParseState.Data;
                    }
                    else if (b == Iac)
                    {
                        if (_subnegotiation.Count < MaxSubnegotiationLength)
                        {
                            _subnegotiation.Add(Iac);
                        }

                        _state = ParseState.Subnegotiation;
                    }
                    else
                    {
                        _state = ParseState.Data; // malformed; resynchronise
                    }

                    break;
            }
        }

        return new Rfc2217Result(data.ToArray(), reply.ToArray());
    }

    /// <summary>
    /// Returns a NOTIFY-MODEMSTATE frame if a masked modem line changed since the last poll (or on the
    /// first poll after the client set a mask), otherwise null.
    /// </summary>
    public byte[]? PollModemState()
    {
        if (_modemMask == 0 || _control() is not { } control)
        {
            return null;
        }

        byte current;
        try
        {
            current = (byte)((control.CtsHolding ? ModemCts : 0) | (control.DsrHolding ? ModemDsr : 0) | (control.CDHolding ? ModemCd : 0));
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or ObjectDisposedException or UnauthorizedAccessException)
        {
            return null;
        }

        byte report;
        if (_lastModem is null)
        {
            report = current;
        }
        else
        {
            var changed = (byte)(current ^ _lastModem.Value);
            byte delta = 0;
            delta |= (byte)((changed & ModemCts) != 0 ? ModemDeltaCts : 0);
            delta |= (byte)((changed & ModemDsr) != 0 ? ModemDeltaDsr : 0);
            delta |= (byte)((changed & ModemCd) != 0 ? ModemDeltaCd : 0);
            if ((delta & _modemMask) == 0)
            {
                return null;
            }

            report = (byte)(current | delta);
        }

        _lastModem = current;
        var frame = new List<byte>();
        Subnegotiation(frame, NotifyModemState, report);
        return frame.ToArray();
    }

    /// <summary>Puts the serial port back to the configured line settings (called when the client leaves).</summary>
    public void Reset()
    {
        if (_control() is not { } control)
        {
            return;
        }

        try
        {
            control.BaudRate = _options.BaudRate;
            control.DataBits = _options.DataBits;
            control.Parity = _options.Parity;
            control.StopBits = _options.StopBits;
            control.Handshake = Handshake.None;
            control.BreakState = false;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or ArgumentException or ObjectDisposedException or UnauthorizedAccessException)
        {
            // Port is gone or rejected the value; the serial loop will reopen with configured settings.
        }
    }

    private static bool IsSupported(byte option) => option is OptionComPort or OptionBinary or OptionSuppressGoAhead;

    private static void Send(List<byte> reply, byte verb, byte option)
    {
        reply.Add(Iac);
        reply.Add(verb);
        reply.Add(option);
    }

    private static void Subnegotiation(List<byte> reply, byte command, params byte[] data)
    {
        reply.AddRange([Iac, Sb, OptionComPort, (byte)(command + ServerOffset)]);
        foreach (var b in data)
        {
            reply.Add(b);
            if (b == Iac)
            {
                reply.Add(Iac);
            }
        }

        reply.AddRange([Iac, Se]);
    }

    private void Ask(List<byte> reply, byte verb, byte option)
    {
        (verb == Will ? _localAsked : _remoteAsked).Add(option);
        Send(reply, verb, option);
    }

    private void Negotiate(byte verb, byte option, List<byte> reply)
    {
        switch (verb)
        {
            case Do:
                Accept(option, _localOn, _localAsked, Will, Wont, reply);
                break;
            case Will:
                Accept(option, _remoteOn, _remoteAsked, Do, Dont, reply);
                break;
            case Dont:
                Refuse(option, _localOn, _localAsked, Wont, reply);
                break;
            case Wont:
                Refuse(option, _remoteOn, _remoteAsked, Dont, reply);
                break;
        }
    }

    private static void Accept(byte option, HashSet<byte> on, HashSet<byte> asked, byte yes, byte no, List<byte> reply)
    {
        if (asked.Remove(option))
        {
            on.Add(option); // answer to our own request; no reply
        }
        else if (on.Contains(option))
        {
            // Already enabled.
        }
        else if (IsSupported(option))
        {
            on.Add(option);
            Send(reply, yes, option);
        }
        else
        {
            Send(reply, no, option);
        }
    }

    private static void Refuse(byte option, HashSet<byte> on, HashSet<byte> asked, byte acknowledge, List<byte> reply)
    {
        if (on.Remove(option))
        {
            Send(reply, acknowledge, option);
        }

        asked.Remove(option);
    }

    private void HandleSubnegotiation(List<byte> reply)
    {
        if (_subnegotiation.Count < 2 || _subnegotiation[0] != OptionComPort)
        {
            return;
        }

        var command = _subnegotiation[1];
        var arg = _subnegotiation.Skip(2).ToArray();
        var control = _control();

        try
        {
            switch (command)
            {
                case Rfc2217Codes.Signature:
                    Subnegotiation(reply, command, global::System.Text.Encoding.ASCII.GetBytes(SignatureText));
                    break;

                case SetBaudRate when arg.Length == 4:
                    var requested = (arg[0] << 24) | (arg[1] << 16) | (arg[2] << 8) | arg[3];
                    if (requested > 0 && control is not null)
                    {
                        control.BaudRate = requested;
                    }

                    var baud = control?.BaudRate ?? _options.BaudRate;
                    Subnegotiation(reply, command, (byte)(baud >> 24), (byte)(baud >> 16), (byte)(baud >> 8), (byte)baud);
                    break;

                case SetDataSize when arg.Length == 1:
                    if (arg[0] is >= 5 and <= 8 && control is not null)
                    {
                        control.DataBits = arg[0];
                    }

                    Subnegotiation(reply, command, (byte)(control?.DataBits ?? _options.DataBits));
                    break;

                case SetParity when arg.Length == 1:
                    if (control is not null && ParityFromCode(arg[0]) is { } parity)
                    {
                        control.Parity = parity;
                    }

                    Subnegotiation(reply, command, ParityCode(control?.Parity ?? _options.Parity));
                    break;

                case SetStopSize when arg.Length == 1:
                    if (control is not null && StopBitsFromCode(arg[0]) is { } stopBits)
                    {
                        control.StopBits = stopBits;
                    }

                    Subnegotiation(reply, command, StopBitsCode(control?.StopBits ?? _options.StopBits));
                    break;

                case SetControl when arg.Length == 1:
                    Subnegotiation(reply, command, ApplyControl(arg[0], control));
                    break;

                case SetLineStateMask when arg.Length == 1:
                    Subnegotiation(reply, command, arg[0]);
                    break;

                case SetModemStateMask when arg.Length == 1:
                    _modemMask = arg[0];
                    _lastModem = null; // report the full state on the next poll
                    Subnegotiation(reply, command, arg[0]);
                    break;

                case PurgeData when arg.Length == 1:
                    if (control is not null)
                    {
                        if (arg[0] is 1 or 3)
                        {
                            control.DiscardInBuffer();
                        }

                        if (arg[0] is 2 or 3)
                        {
                            control.DiscardOutBuffer();
                        }
                    }

                    Subnegotiation(reply, command, arg[0]);
                    break;

                default:
                    // FLOWCONTROL-SUSPEND/RESUME and anything unknown: nothing to do.
                    break;
            }
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or ArgumentException or ObjectDisposedException or UnauthorizedAccessException)
        {
            // The port rejected the setting or vanished; the client simply gets no (or a stale) confirmation.
        }
    }

    private static byte ApplyControl(byte value, ISerialControl? control)
    {
        switch (value)
        {
            case 1 when control is not null:
                control.Handshake = Handshake.None;
                break;
            case 2 when control is not null:
                control.Handshake = Handshake.XOnXOff;
                break;
            case 3 when control is not null:
                control.Handshake = Handshake.RequestToSend;
                break;
            case 5 when control is not null:
                control.BreakState = true;
                break;
            case 6 when control is not null:
                control.BreakState = false;
                break;
            case 8 when control is not null:
                control.DtrEnable = true;
                break;
            case 9 when control is not null:
                control.DtrEnable = false;
                break;
            case 11 when control is not null:
                control.RtsEnable = true;
                break;
            case 12 when control is not null:
                control.RtsEnable = false;
                break;
        }

        return value switch
        {
            0 or 1 or 2 or 3 => control?.Handshake switch
            {
                Handshake.XOnXOff => 2,
                Handshake.RequestToSend or Handshake.RequestToSendXOnXOff => 3,
                _ => 1,
            },
            4 or 5 or 6 => (byte)(control?.BreakState == true ? 5 : 6),
            7 or 8 or 9 => (byte)(control?.DtrEnable == true ? 8 : 9),
            10 or 11 or 12 => (byte)(control?.RtsEnable == true ? 11 : 12),
            _ => value,
        };
    }

    private static byte ParityCode(Parity parity) => parity switch
    {
        Parity.None => 1,
        Parity.Odd => 2,
        Parity.Even => 3,
        Parity.Mark => 4,
        Parity.Space => 5,
        _ => 1,
    };

    private static Parity? ParityFromCode(byte code) => code switch
    {
        1 => Parity.None,
        2 => Parity.Odd,
        3 => Parity.Even,
        4 => Parity.Mark,
        5 => Parity.Space,
        _ => null,
    };

    private static byte StopBitsCode(StopBits stopBits) => stopBits switch
    {
        StopBits.One => 1,
        StopBits.Two => 2,
        StopBits.OnePointFive => 3,
        _ => 1,
    };

    private static StopBits? StopBitsFromCode(byte code) => code switch
    {
        1 => StopBits.One,
        2 => StopBits.Two,
        3 => StopBits.OnePointFive,
        _ => null,
    };
}
