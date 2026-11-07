using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Ports;

/// <summary>Enumerates available serial ports, e.g. for a "--list-ports" CLI action or a GUI port picker.</summary>
public interface ISerialPortDiscovery
{
    IReadOnlyList<string> GetPortNames();

    /// <summary>
    /// A human-readable description per port name (<c>"COM3"</c> → <c>"Prolific USB-to-Serial Comm Port"</c>),
    /// for a picker that wants to say more than the short name — best-effort and never required: a
    /// port with no entry here is still a perfectly good port, just shown by its short name alone.
    /// Keyed case-insensitively. The default is "no descriptions", which is also all a platform with
    /// no wired-up source (see <see cref="SystemSerialPortDiscovery"/>) reports.
    /// </summary>
    IReadOnlyDictionary<string, string> GetPortDescriptions() =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
