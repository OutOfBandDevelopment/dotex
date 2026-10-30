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

/// <summary>
/// The expression picker's state, with no UI framework in it: a searchable list of the paths an expression may read
/// (from <see cref="ValuePathCatalog"/>), the functions the language offers, insert-at-caret editing, live
/// parse/reference diagnostics, and a result evaluated against <see cref="SampleDataGenerator"/> values. The Terminal.Gui and
/// WPF forms render this the way they render the other editor forms. See
/// docs/design/features/expression-picker-paths-and-cel.md.
/// </summary>
/// <summary>What the text being picked is: one expression, a chart's channel list, a semicolon-separated list of expressions, or one value id.</summary>
public enum PickerMode
{
    Expression,
    Channels,
    ExpressionList,
    ValueId,
}
