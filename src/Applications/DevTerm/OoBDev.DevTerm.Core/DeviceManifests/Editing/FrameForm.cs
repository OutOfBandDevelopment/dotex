using System;
using System.Collections.Generic;
using System.ComponentModel;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>The binary frame's own settings: sync prefix and default byte order, plus a read-only summary.</summary>
[FormSection("Frame", Order = 0, Label = "")]
public sealed class FrameForm : EditorForm
{
    public FrameForm(FrameSchema frame, Action edited)
        : base(edited)
    {
        Frame = frame;
    }

    public FrameSchema Frame { get; }

    [Category("Frame")]
    [DisplayName("Sync bytes")]
    [Description("Hex bytes every frame starts with, e.g. A5 5A; blank when frames run back to back.")]
    [FormField(Order = 0)]
    public string? Sync
    {
        get => Frame.Sync;
        set
        {
            Frame.Sync = NullIfBlank(value);
            Changed();
            Notify(nameof(Summary));
        }
    }

    [Category("Frame")]
    [DisplayName("Byte order")]
    [FormField(Order = 1, OptionsFrom = nameof(Endians))]
    public string Endian
    {
        get => Frame.Endian;
        set
        {
            Frame.Endian = value;
            Changed();
        }
    }

    [Browsable(false)]
    public static IReadOnlyList<string> Endians { get; } = ["le", "be"];

    [Category("Frame")]
    [DisplayName("Length field")]
    [Description("Name of an integer field holding the frame's length, for a variable-size last bytes/str field; blank for fixed frames.")]
    [FormField(Order = 2)]
    public string? LengthField
    {
        get => Frame.LengthField;
        set
        {
            Frame.LengthField = NullIfBlank(value);
            Changed();
            Notify(nameof(HasLengthField));
            Notify(nameof(Summary));
        }
    }

    [Category("Frame")]
    [DisplayName("Length adjust")]
    [Description("Added to the length field's value to get the whole frame's length in bytes (0 when it already counts everything).")]
    [FormField(Order = 3, VisibleWhen = nameof(HasLengthField))]
    public int LengthAdjust
    {
        get => Frame.LengthAdjust;
        set
        {
            Frame.LengthAdjust = value;
            Changed();
            Notify(nameof(Summary));
        }
    }

    [Browsable(false)]
    public bool HasLengthField => !string.IsNullOrWhiteSpace(Frame.LengthField);

    [Category("Frame")]
    [DisplayName("Checksum")]
    [Description("The checksum the frame ends with; a frame that fails it is discarded.")]
    [FormField(Order = 4, OptionsFrom = nameof(ChecksumKinds))]
    public string ChecksumKind
    {
        get => Frame.Checksum?.Kind ?? _noChecksum;
        set
        {
            if (string.Equals(value, _noChecksum, StringComparison.Ordinal) || string.IsNullOrWhiteSpace(value))
            {
                Frame.Checksum = null;
            }
            else
            {
                (Frame.Checksum ??= new FrameChecksum()).Kind = value;
            }

            Changed();
            Notify(nameof(HasChecksum));
            Notify(nameof(HasWideChecksum));
            Notify(nameof(Summary));
        }
    }

    [Category("Frame")]
    [DisplayName("Checksum start")]
    [Description("Bytes to skip before the covered data: 0 covers the sync bytes too.")]
    [FormField(Order = 5, VisibleWhen = nameof(HasChecksum))]
    public int ChecksumStart
    {
        get => Frame.Checksum?.Start ?? 0;
        set
        {
            if (Frame.Checksum is { } checksum)
            {
                checksum.Start = value;
                Changed();
                Notify(nameof(Summary));
            }
        }
    }

    [Category("Frame")]
    [DisplayName("Checksum byte order")]
    [Description("Blank uses the kind's usual order.")]
    [FormField(Order = 6, OptionsFrom = nameof(ChecksumEndians), VisibleWhen = nameof(HasWideChecksum))]
    public string ChecksumEndian
    {
        get => Frame.Checksum?.Endian ?? string.Empty;
        set
        {
            if (Frame.Checksum is { } checksum)
            {
                checksum.Endian = NullIfBlank(value);
                Changed();
                Notify(nameof(Summary));
            }
        }
    }

    [Browsable(false)]
    public bool HasChecksum => Frame.Checksum is not null;

    [Browsable(false)]
    public bool HasWideChecksum => Frame.Checksum is { } checksum && FrameChecksum.SizeOf(checksum.Kind) == 2;

    [Browsable(false)]
    public static IReadOnlyList<string> ChecksumKinds { get; } = [_noChecksum, "sum8", "xor8", "crc8", "crc16-modbus", "crc16-ccitt"];

    [Browsable(false)]
    public static IReadOnlyList<string> ChecksumEndians { get; } = ["", "le", "be"];

    private const string _noChecksum = "none";

    [Category("Frame")]
    [DisplayName("Layout")]
    [FormField(Order = 7, Kind = FormFieldKind.Indicator)]
    public string Summary
    {
        get
        {
            var errors = Frame.Validate();
            var length = Frame.Length is { } bytes ? $"{bytes} bytes"
                : Frame.IsVariable ? $"{Frame.MinLength}+ bytes, length from {Frame.LengthField}" : "length unknown";
            return $"{Frame.Fields.Count} field(s), {length}" + (errors.Count == 0 ? string.Empty : "; " + errors[0]);
        }
    }

    internal void Refresh() => Notify(nameof(Summary));
}
