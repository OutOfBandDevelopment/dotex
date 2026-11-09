using System;
using System.Collections.Generic;

namespace OoBDev.Communications.Abstractions;

/// <summary>A raw inbound provider call; signature checks need the exact body bytes.</summary>
/// <param name="Body">The raw request body.</param>
/// <param name="Headers">The request headers.</param>
/// <param name="Query">The query string values.</param>
/// <param name="ReceivedAt">When the host received the call.</param>
public sealed record WebhookRequest(
    ReadOnlyMemory<byte> Body,
    IReadOnlyDictionary<string, string> Headers,
    IReadOnlyDictionary<string, string> Query,
    DateTimeOffset ReceivedAt);
