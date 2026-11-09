using System;
using System.Collections.Generic;
using System.ComponentModel;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One parameter of a command, substituted for <c>{Name}</c> in its template.</summary>
[FormSection("Parameter", Order = 0, Label = "")]
public sealed class ParameterForm : EditorForm
{
    public ParameterForm(CommandParameter parameter, Action edited)
        : base(edited)
    {
        Parameter = parameter;
    }

    public static IReadOnlyList<string> Types { get; } = ["string", "number", "integer"];

    public CommandParameter Parameter { get; }

    public bool IsNumeric => Parameter.IsNumeric;

    [Category("Parameter")]
    [DisplayName("Name")]
    [FormField(Order = 0)]
    public string Name
    {
        get => Parameter.Name;
        set
        {
            Parameter.Name = value;
            Changed();
        }
    }

    [Category("Parameter")]
    [DisplayName("Type")]
    [FormField(Order = 1, OptionsFrom = nameof(Types))]
    public string Type
    {
        get => Parameter.Type;
        set
        {
            Parameter.Type = value;
            Changed(null);
        }
    }

    [Category("Parameter")]
    [DisplayName("Minimum")]
    [FormField(Order = 2, VisibleWhen = nameof(IsNumeric))]
    public double? Minimum
    {
        get => Parameter.Minimum;
        set
        {
            Parameter.Minimum = value;
            Changed();
        }
    }

    [Category("Parameter")]
    [DisplayName("Maximum")]
    [FormField(Order = 3, VisibleWhen = nameof(IsNumeric))]
    public double? Maximum
    {
        get => Parameter.Maximum;
        set
        {
            Parameter.Maximum = value;
            Changed();
        }
    }

    [Category("Parameter")]
    [DisplayName("Unit")]
    [FormField(Order = 4)]
    public string? Unit
    {
        get => Parameter.Unit;
        set
        {
            Parameter.Unit = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Parameter")]
    [DisplayName("Default value")]
    [FormField(Order = 5)]
    public string? DefaultValue
    {
        get => Parameter.DefaultValue;
        set
        {
            Parameter.DefaultValue = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Parameter")]
    [DisplayName("Number format")]
    [Description("A .NET format string, e.g. 00.00; shortest form when blank.")]
    [FormField(Order = 6, VisibleWhen = nameof(IsNumeric))]
    public string? Format
    {
        get => Parameter.Format;
        set
        {
            Parameter.Format = NullIfBlank(value);
            Changed();
        }
    }
}
