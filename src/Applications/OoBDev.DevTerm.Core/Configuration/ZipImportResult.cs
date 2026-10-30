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

/// <summary>Counts from a completed <see cref="ConnectionProfileStore.ImportZip"/> call, for a front end's summary status message.</summary>
public readonly record struct ZipImportResult(int Imported, int Skipped, int Renamed);
