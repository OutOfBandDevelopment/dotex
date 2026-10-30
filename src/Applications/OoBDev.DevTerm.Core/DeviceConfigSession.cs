using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>
/// The read / edit / review / write / confirm flow of one editor, shared by every front end: nothing is written until
/// <see cref="WriteAsync"/>, <see cref="Changes"/> says exactly what will change first, and the write's read-back is compared with what was asked for.
/// </summary>
public sealed class DeviceConfigSession
{
    private readonly IDeviceConfigEditor _editor;
    private readonly Dictionary<string, string> _original = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> _edited = new(StringComparer.Ordinal);

    public DeviceConfigSession(IDeviceConfigEditor editor, DeviceConfigTarget target)
    {
        _editor = editor;
        Target = target;
    }

    public DeviceConfigTarget Target { get; }

    public bool HasRead { get; private set; }

    public bool RebootRequired { get; private set; }

    public IReadOnlyDictionary<string, string> Values => _edited;

    public async Task ReadAsync(CancellationToken cancellationToken = default)
    {
        var values = await _editor.ReadAsync(Target, cancellationToken).ConfigureAwait(false);
        _original.Clear();
        _edited.Clear();
        foreach (var (key, value) in values)
        {
            _original[key] = value;
            _edited[key] = value;
        }

        HasRead = true;
        RebootRequired = false;
    }

    public void Set(string fieldId, string value) => _edited[fieldId] = value;

    /// <summary>Every field whose edited value differs from what was last read.</summary>
    public IReadOnlyList<DeviceConfigChange> Changes() =>
        _edited.Keys.Union(_original.Keys, StringComparer.Ordinal)
            .Select(k => new DeviceConfigChange(k, _original.GetValueOrDefault(k), _edited.GetValueOrDefault(k)))
            .Where(c => !string.Equals(c.From, c.To, StringComparison.Ordinal))
            .OrderBy(c => c.FieldId, StringComparer.Ordinal)
            .ToList();

    public IReadOnlyList<DeviceConfigIssue> Validate() => _editor.Validate(_edited);

    public bool WillDropConnection() => _editor.WillDropConnection(Target, _original, _edited);

    /// <summary>
    /// Writes the edited values (refused when nothing was read yet or validation fails; skipped when nothing changed) and returns the
    /// fields whose read-back differs from what was asked for; empty means the device confirmed everything.
    /// </summary>
    public async Task<IReadOnlyList<DeviceConfigChange>> WriteAsync(CancellationToken cancellationToken = default)
    {
        if (!HasRead)
        {
            throw new InvalidOperationException("Read the device's settings before writing.");
        }

        var issues = Validate();
        if (issues.Count > 0)
        {
            throw new InvalidOperationException("Invalid settings: " + string.Join("; ", issues.Select(i => $"{i.FieldId}: {i.Message}")));
        }

        if (Changes().Count == 0)
        {
            return [];
        }

        var result = await _editor.WriteAsync(Target, _edited, cancellationToken).ConfigureAwait(false);
        var mismatches = _edited
            .Where(kv => !result.ReadBack.TryGetValue(kv.Key, out var back) || !string.Equals(back, kv.Value, StringComparison.Ordinal))
            .Select(kv => new DeviceConfigChange(kv.Key, kv.Value, result.ReadBack.GetValueOrDefault(kv.Key)))
            .ToList();

        _original.Clear();
        foreach (var (key, value) in result.ReadBack)
        {
            _original[key] = value;
        }

        RebootRequired = result.RebootRequired;
        return mismatches;
    }
}
