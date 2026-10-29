using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One device's pane in the Stream Monitor: its name (null for the "all devices" pane) and how many captures it holds.</summary>
public sealed record StreamDevicePane(string? Device, int Count)
{
    /// <summary>The tab or button text: <c>All devices (3)</c> or <c>Bench scope (2)</c>.</summary>
    public string Title => $"{Device ?? "All devices"} ({Count})";
}
