using System.Collections.Generic;
using OoBDev.DevTerm.DeviceManifests.Editing;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The control form fields that take a value expression, a value id or a channel list, and the picker each opens.</summary>
internal static class ExpressionPickerFields
{
    internal static IReadOnlyList<(string Field, PickerMode Mode)> ForControls { get; } =
    [
        (nameof(ControlForm.IndicatorExpression), PickerMode.Expression),
        (nameof(ControlForm.Channels), PickerMode.Channels),
        (nameof(ControlForm.XId), PickerMode.ValueId),
        (nameof(ControlForm.YId), PickerMode.ValueId),
        (nameof(ControlForm.ZId), PickerMode.ValueId),
        (nameof(ControlForm.RadiusId), PickerMode.ValueId),
        (nameof(ControlForm.AngleId), PickerMode.ValueId),
        (nameof(ControlForm.HueId), PickerMode.ValueId),
        (nameof(ControlForm.SaturationId), PickerMode.ValueId),
        (nameof(ControlForm.BrightnessId), PickerMode.ValueId),
        (nameof(ControlForm.VisibleWhenId), PickerMode.ValueId),
        (nameof(ControlForm.ParameterExpressions), PickerMode.ExpressionList),
    ];
}
