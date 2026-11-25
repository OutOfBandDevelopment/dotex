using System;
namespace OoBDev.DevTerm;

/// <summary>One detected, captured piece of renderable/binary content from a session's incoming bytes.</summary>
/// <param name="Kind">What it was detected (or declared) as.</param>
/// <param name="Data">The captured bytes — the content itself, with any SCPI definite-length block header already stripped.</param>
/// <param name="StartedAt">When the first byte of it arrived.</param>
/// <param name="EndReason">Why the capture ended; anything but <see cref="StreamCaptureEnd.Complete"/> may mean it's truncated.</param>
/// <param name="WasDeclared">Whether a declared response-format hint (see <see cref="IStreamContentHintSink"/>) identified it, rather than sniffing alone.</param>
public sealed record StreamCapture(StreamContentKind Kind, byte[] Data, DateTimeOffset StartedAt, StreamCaptureEnd EndReason, bool WasDeclared);
