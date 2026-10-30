using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>The kind of value a <see cref="ValuePath"/> holds, as far as the manifest can tell.</summary>
public enum ValuePathType
{
    Number,
    Text,
    Boolean,
}
