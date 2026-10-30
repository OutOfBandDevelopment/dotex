using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OoBDev.DevTerm;

/// <summary>One remembered approval (<see cref="PluginTrust.Approvals"/>).</summary>
public sealed record PluginApproval(string Name, string Hash, DateTimeOffset ApprovedAt);
