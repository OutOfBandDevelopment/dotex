using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Logging;

/// <summary>A playback speed choice: its label and rate (<see cref="double.PositiveInfinity"/> for Max).</summary>
public sealed record PlaybackSpeed(string Label, double Rate)
{
    public override string ToString() => Label;
}
