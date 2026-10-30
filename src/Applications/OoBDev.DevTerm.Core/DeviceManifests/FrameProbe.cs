using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>What <see cref="FrameDecoder.Probe"/> found at the start of a buffer.</summary>
public enum FrameProbe
{
    /// <summary>A whole frame's length is known and the buffer holds it.</summary>
    Ready,

    /// <summary>More bytes are needed before the frame's length (or the frame itself) is known.</summary>
    NeedMore,

    /// <summary>The bytes can't start a frame (a length outside what the schema allows): drop a byte and look again.</summary>
    Invalid,
}
