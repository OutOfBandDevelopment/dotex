using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>One validation complaint about a settings value, keyed by the form field id it concerns.</summary>
public sealed record DeviceConfigIssue(string FieldId, string Message);
