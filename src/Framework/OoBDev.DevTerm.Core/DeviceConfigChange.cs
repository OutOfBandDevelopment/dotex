using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>One changed value in a pending write.</summary>
public sealed record DeviceConfigChange(string FieldId, string? From, string? To);
