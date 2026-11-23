using System;
using OoBDev.System.IO;

namespace OoBDev.DevTerm;

/// <summary>A transport an approved <c>process</c> plugin of kind <c>transport</c> offers; chosen by <c>--transport {Name}</c>.</summary>
/// <param name="Name">The plugin name, matched case-insensitively against the transport option.</param>
/// <param name="Create">Builds a fresh adapter for one session.</param>
public sealed record PluginTransport(string Name, Func<IDeviceAdapter> Create);
