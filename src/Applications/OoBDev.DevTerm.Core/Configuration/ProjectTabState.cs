using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>The live state of one open tab, as captured into a project: its connection plus its send history and whether it was logging.</summary>
public sealed record ProjectTabState(string Name, CliOptions Options, IReadOnlyList<string> History, bool Logging);
