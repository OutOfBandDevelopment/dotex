using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One function the expression language offers, with the snippet the picker inserts for it.</summary>
public sealed record ExpressionFunction(string Name, string Signature, string Description, string Snippet, int CaretFromEnd = 1);
