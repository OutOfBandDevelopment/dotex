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

/// <summary>The outcome of one <see cref="StreamCaptureConverter.ConvertAsync"/> call.</summary>
public sealed record StreamConversionResult(bool Success, string? OutputPath, string? Error);
