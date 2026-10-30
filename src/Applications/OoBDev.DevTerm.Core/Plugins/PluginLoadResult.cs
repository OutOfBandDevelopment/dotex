using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.DevTerm;

/// <summary>One plugin folder's outcome: loaded, or skipped with the reason.</summary>
public sealed record PluginLoadResult(string Folder, string Name, bool Loaded, string Message);
