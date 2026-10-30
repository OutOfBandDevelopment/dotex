using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.DeviceManifests;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.DevTerm.DeviceManifests.Editing;

/// <summary>The manifest's own fields: identity, the transport hint, the command terminator, and how replies arrive.</summary>
[FormSection("Identity", Order = 0)]
[FormSection("Connection", Order = 1)]
[FormSection("Replies", Order = 2)]
public sealed class ManifestIdentityForm : EditorForm
{
    private readonly DeviceManifest _manifest;

    public ManifestIdentityForm(DeviceManifest manifest, Action edited)
        : base(edited)
    {
        _manifest = manifest;
    }

    public static IReadOnlyList<string> TransportTypes { get; } = ["", "serial", "tcp", "hid", "usbtmc", "rfc2217", "vxi11", "mqtt", "amqp", "stomp", "loopback"];

    [Category("Identity")]
    [DisplayName("Name")]
    [Description("What pickers and the panel's title show.")]
    [FormField(Order = 0)]
    public string Name
    {
        get => _manifest.Name;
        set
        {
            _manifest.Name = value;
            Changed();
        }
    }

    [Category("Identity")]
    [DisplayName("Vendor")]
    [FormField(Order = 1)]
    public string? Vendor
    {
        get => _manifest.Vendor;
        set
        {
            _manifest.Vendor = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Identity")]
    [DisplayName("Version")]
    [FormField(Order = 2)]
    public string? Version
    {
        get => _manifest.Version;
        set
        {
            _manifest.Version = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Identity")]
    [DisplayName("Description")]
    [Description("Shown as the panel's Notes when the panel has none of its own.")]
    [FormField(Order = 3)]
    public string? Description
    {
        get => _manifest.Description;
        set
        {
            _manifest.Description = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Identity")]
    [DisplayName("Panel file")]
    [Description("Package mode: the panel is kept in this file beside the manifest (e.g. ui.json) instead of inline.")]
    [FormField(Order = 4)]
    public string? UiFile
    {
        get => _manifest.UiFile;
        set
        {
            _manifest.UiFile = NullIfBlank(value);
            Changed();
        }
    }

    [Category("Connection")]
    [DisplayName("Transport hint")]
    [Description("A suggestion for the connection this device needs; blank for none.")]
    [FormField(Order = 0, OptionsFrom = nameof(TransportTypes))]
    public string TransportType
    {
        get => _manifest.Transport?.Type ?? string.Empty;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _manifest.Transport = null;
            }
            else if (_manifest.Transport is { } hint)
            {
                hint.Type = value;
            }
            else
            {
                _manifest.Transport = new TransportHint { Type = value };
            }

            Changed();
        }
    }

    [Category("Connection")]
    [DisplayName("Transport options")]
    [Description("Key=value pairs, comma-separated (e.g. Baud=9600, DataBits=8).")]
    [FormField(Order = 1, VisibleWhen = nameof(HasTransport))]
    public string TransportOptions
    {
        get => string.Join(", ", (_manifest.Transport?.Options ?? []).Select(o => $"{o.Key}={o.Value}"));
        set
        {
            if (_manifest.Transport is not { } hint)
            {
                return;
            }

            hint.Options = [.. FormBinding.SplitList(value)
                .Select(pair => pair.Split('=', 2))
                .Where(kv => kv[0].Trim().Length > 0)
                .Select(kv => new TransportOption { Key = kv[0].Trim(), Value = kv.Length > 1 ? kv[1].Trim() : string.Empty })];
            Changed();
        }
    }

    public bool HasTransport => _manifest.Transport is not null;

    [Category("Connection")]
    [DisplayName("Command terminator")]
    [Description("Appended to every command sent, written with escapes: \\n, \\r\\n, \\x03 ...")]
    [FormField(Order = 2)]
    public string Terminator
    {
        get => Escape(_manifest.Terminator);
        set
        {
            _manifest.Terminator = Unescape(value);
            Changed();
        }
    }

    [Category("Replies")]
    [DisplayName("Replies end in CR/LF")]
    [Description("Off for a device whose replies have no terminator: each read is one reply.")]
    [FormField(Order = 0)]
    public bool LineTerminated
    {
        get => _manifest.Inbound?.LineTerminated ?? true;
        set
        {
            (_manifest.Inbound ??= new InboundProtocol()).LineTerminated = value;
            Changed();
        }
    }

    [Category("Replies")]
    [DisplayName("Kaitai layout file")]
    [Description("A binary protocol's .ksy file, beside the manifest (referenced only).")]
    [FormField(Order = 1)]
    public string? KaitaiFile
    {
        get => _manifest.Inbound?.KaitaiFile;
        set
        {
            var file = NullIfBlank(value);
            if (file is null && _manifest.Inbound is null)
            {
                return;
            }

            (_manifest.Inbound ??= new InboundProtocol()).KaitaiFile = file;
            Changed();
        }
    }
}
