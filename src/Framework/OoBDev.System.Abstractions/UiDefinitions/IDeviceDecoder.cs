using System.Buffers;
using System.Collections.Generic;

namespace OoBDev.UiDefinitions;

/// <summary>
/// Decodes a device's received bytes into text lines and publishes the named live values behind them
/// (<see cref="IStructuredPresenter"/>). A front end exposes it as a presenter named <see cref="Name"/>.
/// </summary>
public interface IDeviceDecoder : IStructuredPresenter
{
    /// <summary>The presenter name that selects this decoder, for example <c>k8055</c>.</summary>
    string Name { get; }

    /// <summary>Decodes a chunk; buffers a partial frame and returns a line per complete frame.</summary>
    /// <param name="data">The received bytes.</param>
    IReadOnlyList<string> Decode(ReadOnlySequence<byte> data);
}
