using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Scpi;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Devices.Scpi;

/// <summary>
/// <see cref="IControlSurface"/> for a SCPI profile: a button id resolves to the profile's command, its parameter values arrive as one
/// joined value, and a query registers its reply indicator with the presenter before the bytes go out.
/// </summary>
public sealed class ScpiControlSurface : IControlSurface, ICommandPreview
{
    /// <summary>The id of the "Send" button of the custom-command row.</summary>
    public const string SendCustomCommandId = "sendCustom";

    private readonly Session _session;
    private readonly ScpiInstrumentProfile _profile;
    private readonly IReplyTracker? _tracker;
    private readonly Dictionary<string, ScpiCommandDefinition> _commandsById;
    private readonly HashSet<string> _parameterFieldIds;

    /// <summary>Creates the surface.</summary>
    /// <param name="session">The session that sends the bytes.</param>
    /// <param name="profile">The instrument profile.</param>
    /// <param name="tracker">The presenter that matches replies to indicators, when one is running.</param>
    public ScpiControlSurface(Session session, ScpiInstrumentProfile profile, IReplyTracker? tracker)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(profile);

        _session = session;
        _profile = profile;
        _tracker = tracker;
        _commandsById = profile.Commands.ToDictionary(c => c.Id, StringComparer.Ordinal);

        // Parameter fields only hold values for their button; the renderers still invoke a field by its own id when it is committed.
        _parameterFieldIds =
        [
            .. profile.Commands.Where(c => c.Parameters.Count > 0).SelectMany(c => c.Parameters.Select(p => $"{c.Id}.{p.Name}")),
            ScpiUiDefinitionBuilder.CustomCommandFieldId,
        ];
    }

    /// <inheritdoc />
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        if (Resolve(commandId, value) is not { } resolved)
        {
            return Task.CompletedTask;
        }

        if (resolved.ReplyIndicatorId is { } replyIndicatorId)
        {
            _tracker?.QuerySent(replyIndicatorId);
        }

        if (resolved.Request.ResponseFormat == ScpiResponseFormat.Image)
        {
            foreach (var sink in _session.Presenters.OfType<IStreamContentHintSink>())
            {
                sink.ExpectResponse(StreamContentFormat.Image);
            }
        }

        return SendAsync(Encoding.ASCII.GetBytes(resolved.Request.WireText), resolved.ReplyIndicatorId, cancellationToken);
    }

    /// <inheritdoc />
    public string? PreviewCommand(string commandId, string? value)
    {
        ArgumentNullException.ThrowIfNull(commandId);

        if (commandId != SendCustomCommandId && !_parameterFieldIds.Contains(commandId) && !_commandsById.ContainsKey(commandId))
        {
            return null;
        }

        return Resolve(commandId, value) is { } resolved ? CommandPreviewFormat.EscapeControlCharacters(resolved.Request.WireText) : null;
    }

    private async Task SendAsync(byte[] bytes, string? replyIndicatorId, CancellationToken cancellationToken)
    {
        try
        {
            await _session.SendAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            if (replyIndicatorId is not null)
            {
                _tracker?.Cancel(replyIndicatorId);
            }

            throw;
        }
    }

    private (ScpiRequest Request, string? ReplyIndicatorId)? Resolve(string commandId, string? value)
    {
        if (commandId == SendCustomCommandId)
        {
            return (_profile.BuildCustom(value ?? string.Empty, isQuery: true), $"{SendCustomCommandId}.reply");
        }

        if (_parameterFieldIds.Contains(commandId))
        {
            return null;
        }

        if (!_commandsById.TryGetValue(commandId, out var command))
        {
            throw new ArgumentException($"Unknown SCPI command '{commandId}'.", nameof(commandId));
        }

        var request = _profile.Build(command.Id, ParameterValueList.Split(value));
        return (request, command.IsQuery ? $"{command.Id}.reply" : null);
    }
}
