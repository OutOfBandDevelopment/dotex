using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>
/// One outbound command: named parameters templated into a send template (e.g. "SOUR:VOLT {value}\r").
/// A control invokes it by <see cref="EffectiveId"/> (a <c>UiControl.Id</c>, or a button's
/// <c>CommandId</c>); its value arrives comma-joined when a button reads several parameter fields.
/// </summary>
public sealed class OutboundCommand
{
    public required string Name { get; set; }

    /// <summary>The command id a control invokes; <see cref="Name"/> when unset.</summary>
    public string? Id { get; set; }

    public List<CommandParameter> Parameters { get; set; } = [];

    public required string Template { get; set; }

    /// <summary>Whether the next reply line belongs to this command — shown by the indicator <see cref="EffectiveReplyId"/>.</summary>
    public bool IsQuery { get; set; }

    /// <summary>A question the control panel asks before sending this command (Yes/No), for one that is hard to undo; null sends at once.</summary>
    public string? ConfirmMessage { get; set; }

    /// <summary>
    /// A regular expression the reply line must match to answer this query. Without it the next line is the reply (FIFO);
    /// with it, only a matching line is, and any other line is shown as unsolicited telemetry. Ignored unless the command is a query.
    /// </summary>
    public string? ReplyPattern { get; set; }

    /// <summary>The indicator id a query's reply publishes to; <c>{EffectiveId}.reply</c> when unset (the SCPI module's convention).</summary>
    public string? ReplyId { get; set; }

    /// <summary>
    /// What this command's reply is expected to contain, same vocabulary as an SCPI profile command. Anything other than
    /// <see cref="StreamContentFormat.Text"/> tells the Stream Monitor (via the session's hint sinks) to treat the next reply as that
    /// format instead of sniffing it.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<StreamContentFormat>))]
    public StreamContentFormat ExpectedResponseFormat { get; set; } = StreamContentFormat.Text;

    /// <summary><see cref="Id"/>, else <see cref="Name"/>.</summary>
    [JsonIgnore]
    public string EffectiveId => string.IsNullOrWhiteSpace(Id) ? Name : Id;

    /// <summary>The reply indicator id for a query (<see cref="IsQuery"/> or an explicit <see cref="ReplyId"/>); null otherwise.</summary>
    [JsonIgnore]
    public string? EffectiveReplyId => ReplyId ?? (IsQuery ? $"{EffectiveId}.reply" : null);
}
