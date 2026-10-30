using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>What a write did: the values read back from the device afterwards, and whether the device must be restarted for them to apply.</summary>
public sealed record DeviceConfigWriteResult(IReadOnlyDictionary<string, string> ReadBack, bool RebootRequired);
