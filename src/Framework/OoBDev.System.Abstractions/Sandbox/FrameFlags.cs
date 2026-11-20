using System;

namespace OoBDev.System.Sandbox;

/// <summary>Optional protections a frame carries. The host fixes the required set for a session, and a frame missing one is a violation.</summary>
[Flags]
public enum FrameFlags : byte
{
    /// <summary>Header and body only.</summary>
    None = 0,

    /// <summary>A CRC-32 of header and body follows the body. It catches corruption, not tampering.</summary>
    Crc32 = 1,

    /// <summary>An HMAC-SHA256 under the session key follows. It authenticates header and body.</summary>
    Hmac = 2,

    /// <summary>A response names the request it answers and folds that request's HMAC into its own, so it cannot be replayed against another request. Needs <see cref="Hmac"/>.</summary>
    CounterSign = 4,
}
