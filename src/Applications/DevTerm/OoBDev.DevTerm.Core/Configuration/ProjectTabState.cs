using System.Collections.Generic;

namespace OoBDev.DevTerm.Configuration;

/// <summary>The live state of one open tab, as captured into a project: its connection plus its send history and whether it was logging.</summary>
public sealed record ProjectTabState(string Name, CliOptions Options, IReadOnlyList<string> History, bool Logging);
