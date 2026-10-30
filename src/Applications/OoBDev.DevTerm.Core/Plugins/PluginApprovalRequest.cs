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

/// <summary>What the user is asked to approve: a program that will run with their rights.</summary>
public sealed record PluginApprovalRequest(string Name, string Version, string Folder, string CommandLine, string Hash);
