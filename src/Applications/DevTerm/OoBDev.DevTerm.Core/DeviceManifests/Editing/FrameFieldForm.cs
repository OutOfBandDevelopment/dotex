using System;
using System.Collections.Generic;
using System.ComponentModel;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One field of the binary frame.</summary>
[FormSection("Field", Order = 0, Label = "")]
public sealed class FrameFieldForm : EditorForm
{
    private readonly Action _changedSummary;

    public FrameFieldForm(FrameField field, Action edited, Action changedSummary)
        : base(edited)
    {
        Field = field;
        _changedSummary = changedSummary;
    }

    public FrameField Field { get; }

    [Browsable(false)]
    public static IReadOnlyList<string> Types { get; } = ["u1", "u2", "u4", "u8", "s1", "s2", "s4", "s8", "f4", "f8", "b1", "b2", "b3", "b4", "b5", "b6", "b7", "b8", "b12", "b16", "str", "bytes", "skip"];

    [Browsable(false)]
    public static IReadOnlyList<string> Endians { get; } = ["", "le", "be"];

    [Browsable(false)]
    public bool NeedsSize => !Field.IsNumber;

    [Browsable(false)]
    public bool IsNumber => Field.IsNumber;

    [Browsable(false)]
    public bool IsPublished => Field.Publishes;

    [Category("Field")]
    [DisplayName("Name")]
    [Description("The value id this field publishes; dotted (header.length) or indexed (samples[0]) names are fine.")]
    [FormField(Order = 0, VisibleWhen = nameof(IsPublished))]
    public string Name
    {
        get => Field.Name;
        set
        {
            Field.Name = value;
            Changed();
        }
    }

    [Category("Field")]
    [DisplayName("Type")]
    [FormField(Order = 1, OptionsFrom = nameof(Types))]
    public string Type
    {
        get => Field.Type;
        set
        {
            Field.Type = value;
            Changed(null);
            _changedSummary();
        }
    }

    [Category("Field")]
    [DisplayName("Size (bytes)")]
    [Description("Required for str, bytes and skip (leave blank on a last bytes/str field when the frame takes its length from a field).")]
    [FormField(Order = 2, VisibleWhen = nameof(NeedsSize))]
    public double? Size
    {
        get => Field.Size;
        set
        {
            Field.Size = value is { } size ? (int)Math.Round(size) : null;
            Changed();
            _changedSummary();
        }
    }

    [Category("Field")]
    [DisplayName("Byte order")]
    [Description("Blank uses the frame's.")]
    [FormField(Order = 3, OptionsFrom = nameof(Endians), VisibleWhen = nameof(IsNumber))]
    public string EndianOverride
    {
        get => Field.Endian ?? string.Empty;
        set
        {
            Field.Endian = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Field")]
    [DisplayName("Scale")]
    [Description("The raw number is multiplied by this, then Offset is added.")]
    [FormField(Order = 4, VisibleWhen = nameof(IsNumber))]
    public double? Scale
    {
        get => Field.Scale;
        set
        {
            Field.Scale = value;
            Changed();
        }
    }

    [Category("Field")]
    [DisplayName("Offset")]
    [FormField(Order = 5, VisibleWhen = nameof(IsNumber))]
    public double? Offset
    {
        get => Field.Offset;
        set
        {
            Field.Offset = value;
            Changed();
        }
    }

    [Category("Field")]
    [DisplayName("Unit")]
    [FormField(Order = 6, VisibleWhen = nameof(IsPublished))]
    public string? Unit
    {
        get => Field.Unit;
        set
        {
            Field.Unit = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Field")]
    [DisplayName("Label")]
    [FormField(Order = 7, VisibleWhen = nameof(IsPublished))]
    public string? Label
    {
        get => Field.Label;
        set
        {
            Field.Label = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Field")]
    [DisplayName("Must equal")]
    [Description("Hex bytes this field must hold (a magic number); a frame that differs is discarded.")]
    [FormField(Order = 8)]
    public string? Expect
    {
        get => Field.Expect;
        set
        {
            Field.Expect = NullIfBlank(value);
            Changed();
        }
    }
}
