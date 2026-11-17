using System;
using System.Buffers;
using System.Collections.Generic;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Panels;

/// <summary>Exposes an <see cref="IDeviceDecoder"/> as an <see cref="IPresenter"/> that also forwards its live values.</summary>
public sealed class DeviceDecoderPresenter : IPresenter, IStructuredPresenter
{
    private readonly IDeviceDecoder _decoder;

    /// <summary>Wraps <paramref name="decoder"/>.</summary>
    /// <param name="decoder">The decoder.</param>
    public DeviceDecoderPresenter(IDeviceDecoder decoder)
    {
        ArgumentNullException.ThrowIfNull(decoder);
        _decoder = decoder;
    }

    /// <summary>The wrapped decoder.</summary>
    public IDeviceDecoder Decoder => _decoder;

    /// <inheritdoc />
    public string Name => _decoder.Name;

    /// <inheritdoc />
    public event EventHandler<IReadOnlyDictionary<string, string>>? ValuesChanged
    {
        add => _decoder.ValuesChanged += value;
        remove => _decoder.ValuesChanged -= value;
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Render(ReadOnlySequence<byte> data) => _decoder.Decode(data);
}
