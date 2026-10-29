using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>Which mechanism <see cref="StreamCaptureConverter"/> uses - see its own doc comment.</summary>
public enum StreamConversionMode
{
    /// <summary>No conversion mechanism is configured; <see cref="StreamCaptureConverter.ConvertAsync"/> always fails with an explanatory message.</summary>
    None,

    /// <summary>Run a configured external executable (Ghostscript-style) against the capture's saved file.</summary>
    ExternalTool,

    /// <summary>dev-term's own <see cref="HpglToSvgConverter"/> - HP-GL captures only.</summary>
    InternalHpglToSvg,

    /// <summary>Run the first registered tool (<see cref="StreamCaptureConverterOptions.Tools"/>) whose formats include the capture's.</summary>
    Auto,

    /// <summary>Run the registered tool named <see cref="StreamCaptureConverterOptions.ToolName"/>.</summary>
    Tool,
}
