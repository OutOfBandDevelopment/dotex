using System;
using System.ComponentModel;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>One outbound command: what it's called, what it sends, and whether a reply belongs to it.</summary>
[FormSection("Command", Order = 0, Label = "")]
public sealed class CommandForm : EditorForm
{
    private readonly DeviceManifest _manifest;

    public CommandForm(DeviceManifest manifest, OutboundCommand command, Action edited)
        : base(edited)
    {
        _manifest = manifest;
        Command = command;
    }

    public OutboundCommand Command { get; }

    [Category("Command")]
    [DisplayName("Name")]
    [FormField(Order = 0)]
    public string Name
    {
        get => Command.Name;
        set
        {
            Command.Name = value;
            Changed();
        }
    }

    [Category("Command")]
    [DisplayName("Id")]
    [Description("What a panel control invokes; the name when blank.")]
    [FormField(Order = 1)]
    public string? Id
    {
        get => Command.Id;
        set
        {
            Command.Id = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Command")]
    [DisplayName("Template")]
    [Description("The text sent, with {name} for each parameter ({value} when there are none); escapes like \\r allowed.")]
    [FormField(Order = 2)]
    public string Template
    {
        get => Escape(Command.Template);
        set
        {
            Command.Template = Unescape(value);
            Changed();
        }
    }

    [Category("Command")]
    [DisplayName("Is a query")]
    [Description("The next reply line belongs to this command and shows in its reply indicator.")]
    [FormField(Order = 3)]
    public bool IsQuery
    {
        get => Command.IsQuery;
        set
        {
            Command.IsQuery = value;
            Changed();
        }
    }

    [Category("Command")]
    [DisplayName("Reply id")]
    [Description("The indicator a query's reply lands in; {id}.reply when blank.")]
    [FormField(Order = 4)]
    public string? ReplyId
    {
        get => Command.ReplyId;
        set
        {
            Command.ReplyId = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Command")]
    [DisplayName("Sends")]
    [FormField(Order = 5, Kind = FormFieldKind.Indicator)]
    public string Sends => EditorForm.Escape(ManifestControlSurface.FormatTemplate(Command, null) + _manifest.Terminator)
        + (Command.EffectiveReplyId is { } reply ? $"   (reply → {reply})" : string.Empty);
}
