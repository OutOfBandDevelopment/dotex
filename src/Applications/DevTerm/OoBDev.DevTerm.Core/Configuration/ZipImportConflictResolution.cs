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

/// <summary>How <see cref="ConnectionProfileStore.ImportZip"/> handles a zip entry whose name already matches a saved profile.</summary>
public enum ZipImportConflictResolution
{
    /// <summary>Leave the existing profile alone; don't import this entry.</summary>
    Skip,

    /// <summary>Import this entry under a new, non-conflicting name (e.g. <c>"name (2)"</c>).</summary>
    Rename,

    /// <summary>Overwrite the existing profile with this entry.</summary>
    Replace,
}
