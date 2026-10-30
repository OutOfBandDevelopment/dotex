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

/// <summary>Where a <see cref="ValuePath"/> comes from in the manifest.</summary>
public enum ValuePathSource
{
    /// <summary>A response pattern's name, or one of its named capture groups.</summary>
    ResponsePattern,

    /// <summary>A field of the manifest's binary frame (<see cref="InboundProtocol.Frame"/>).</summary>
    Frame,

    /// <summary>The id a query command's reply is published under.</summary>
    QueryReply,

    /// <summary>A panel control's id (an indicator is a published value; every control's current value is readable by a button's parameter expressions).</summary>
    Control,
}
