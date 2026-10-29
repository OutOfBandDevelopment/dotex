using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>The device control panels a main window's Device menu offers.</summary>
public enum DevicePanel
{
    K8055,
    Busylight,

    /// <summary>A panel built from a loaded device manifest (Device > Device Manifest...) — any connection.</summary>
    Manifest,
}
