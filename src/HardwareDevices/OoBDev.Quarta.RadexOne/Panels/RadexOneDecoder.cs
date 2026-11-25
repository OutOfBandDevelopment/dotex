using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using OoBDev.UiDefinitions;

namespace OoBDev.Quarta.RadexOne.Panels;

/// <summary>Cuts the received stream into reply packets (dropping noise a byte at a time, like <see cref="RadexOneSegmenter"/>), writes one text line per reply and publishes the values it carries.</summary>
public sealed class RadexOneDecoder : IDeviceDecoder
{
    private readonly List<byte> _buffer = [];
    private readonly RadexOneMessageDecoder _decoder = new();

    /// <inheritdoc />
    public string Name => QuartaGlobals.ProviderKey;

    /// <inheritdoc />
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged;

    /// <inheritdoc />
    public IReadOnlyList<string> Decode(ReadOnlySequence<byte> data)
    {
        foreach (var segment in data)
        {
            _buffer.AddRange(segment.Span.ToArray());
        }

        var lines = new List<string>();
        while (TryTakePacket(out var packet))
        {
            lines.Add(Describe(_decoder.Decode(new ReadOnlySequence<byte>(packet)), out var values));
            if (values.Count > 0)
            {
                ValuesChanged?.Invoke(this, values);
            }
        }

        return lines;
    }

    private bool TryTakePacket(out byte[] packet)
    {
        packet = [];
        while (_buffer.Count >= 2)
        {
            if (_buffer[0] != RadexOneFramer.InboundPrefix0 || _buffer[1] != RadexOneFramer.InboundPrefix1)
            {
                _buffer.RemoveAt(0);
                continue;
            }

            if (_buffer.Count < RadexOneFramer.HeaderLength)
            {
                return false;
            }

            if (!RadexOneFramer.TryValidateHeader(_buffer.GetRange(0, RadexOneFramer.HeaderLength).ToArray(), out var extensionLength))
            {
                _buffer.RemoveAt(0);
                continue;
            }

            var total = RadexOneFramer.HeaderLength + extensionLength;
            if (_buffer.Count < total)
            {
                return false;
            }

            packet = _buffer.GetRange(0, total).ToArray();
            _buffer.RemoveRange(0, total);
            return true;
        }

        return false;
    }

    private static string Describe(RadexOneReply reply, out Dictionary<string, string> values)
    {
        values = [];
        if (reply.TryGetReadData(out var data))
        {
            values["ambient"] = data.Ambient.ToString(CultureInfo.InvariantCulture);
            values["accumulated"] = data.Accumulated.ToString(CultureInfo.InvariantCulture);
            values["cpm"] = data.Cpm.ToString(CultureInfo.InvariantCulture);
            return $"DATA: ambient={data.Ambient} accumulated={data.Accumulated} cpm={data.Cpm}";
        }

        if (reply.TryGetSettings(out var settings))
        {
            values["alarmMode"] = settings.AlarmMode.ToString();
            values["threshold"] = settings.Threshold.ToString(CultureInfo.InvariantCulture);
            return $"SETTINGS: alarm={settings.AlarmMode} threshold={settings.Threshold}";
        }

        if (reply.SerialVersionText is { } text)
        {
            values["serialVersion"] = text;
            return $"SERIAL: {text}";
        }

        return reply.IsAcknowledgement
            ? $"ACK: 0x{reply.CommandCode:X4}"
            : $"REPLY: command 0x{reply.CommandCode ?? 0:X4} packet {reply.PacketNumber}";
    }
}
