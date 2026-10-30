using System.Collections.Generic;

namespace OoBDev.System.IO.Devices;

/// <summary>What a write did: the values read back from the device afterwards, and whether the device must be restarted for them to apply.</summary>
public sealed record DeviceConfigWriteResult(IReadOnlyDictionary<string, string> ReadBack, bool RebootRequired);
