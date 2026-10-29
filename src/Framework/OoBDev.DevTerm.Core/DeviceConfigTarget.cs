using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>Where a configuration editor reads and writes: a host/port (UDP or TCP, the editor decides) typed or taken from network discovery.</summary>
public sealed record DeviceConfigTarget(string Host, int Port = 0);
