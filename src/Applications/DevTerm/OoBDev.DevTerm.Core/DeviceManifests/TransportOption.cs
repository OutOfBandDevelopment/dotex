using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>One key/value pair in a <see cref="TransportHint"/>.</summary>
public sealed class TransportOption
{
    public required string Key { get; set; }

    public required string Value { get; set; }
}
