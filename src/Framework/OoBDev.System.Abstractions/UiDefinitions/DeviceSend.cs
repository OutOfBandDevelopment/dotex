using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.UiDefinitions;

/// <summary>Sends raw bytes to the connected device; how a <see cref="IControlSurface"/> reaches the wire without knowing the session behind it.</summary>
/// <param name="data">The bytes to write.</param>
/// <param name="cancellationToken">Cancels the send.</param>
public delegate Task DeviceSend(ReadOnlyMemory<byte> data, CancellationToken cancellationToken);
