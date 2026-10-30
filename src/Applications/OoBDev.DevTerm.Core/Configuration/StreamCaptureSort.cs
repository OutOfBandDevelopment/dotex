using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>How the Stream Monitor's capture list is ordered.</summary>
public enum StreamCaptureSort
{
    /// <summary>Oldest first - the monitor's own order.</summary>
    Oldest,
    Newest,
    Largest,
    Kind,
    Device,
}
