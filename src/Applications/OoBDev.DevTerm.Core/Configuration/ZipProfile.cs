using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One profile read out of a zip by <see cref="ConnectionProfileStore.ReadZip"/>: its name and its raw JSON text.</summary>
public sealed record ZipProfile(string Name, string Json);
