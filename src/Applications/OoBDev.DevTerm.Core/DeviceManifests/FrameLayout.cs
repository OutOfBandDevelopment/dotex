using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>The resolved layout of a <see cref="FrameSchema"/>: the fixed bytes up to any variable tail, plus the checksum trailer.</summary>
internal sealed record FrameLayout(IReadOnlyList<FrameSlot> Slots, int FixedBytes, int TailIndex, int ChecksumBytes)
{
    /// <summary>The shortest frame: everything fixed, an empty tail, and the checksum.</summary>
    public int MinLength => FixedBytes + ChecksumBytes;
}
