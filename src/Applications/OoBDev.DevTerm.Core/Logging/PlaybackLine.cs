using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Logging;

/// <summary>One display line of playback output.</summary>
public sealed record PlaybackLine(string Text, PlaybackLineKind Kind)
{
    public override string ToString() => Text;
}
