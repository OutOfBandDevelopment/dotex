using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>
/// Enumerates every path an <see cref="Expression"/> in a <see cref="DeviceManifest"/> can reference, from the manifest alone
/// (no connected device). It is the single source for the expression picker, sample-data generation and the validator's
/// unknown-reference check. See docs/design/UiDefinitions/expression-picker-paths-and-cel.md.
/// </summary>
public static partial class ValuePathCatalog
{
    /// <summary>Every readable path, de-duplicated by <see cref="ValuePath.Path"/> (first source wins), in a stable order: patterns, then frame fields, then query replies, then controls.</summary>
    public static IReadOnlyList<ValuePath> Enumerate(DeviceManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var paths = new List<ValuePath>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        void Add(ValuePath path)
        {
            if (!string.IsNullOrWhiteSpace(path.Path) && seen.Add(path.Path))
            {
                paths.Add(path);
            }
        }

        foreach (var pattern in manifest.Inbound?.Patterns ?? [])
        {
            foreach (var path in FromPattern(pattern))
            {
                Add(path);
            }
        }

        foreach (var field in manifest.Inbound?.Frame?.Fields ?? [])
        {
            if (field.Publishes && !string.IsNullOrWhiteSpace(field.Name))
            {
                Add(new ValuePath(field.Name, field.IsNumber ? ValuePathType.Number : ValuePathType.Text, ValuePathSource.Frame, field.Label ?? field.Name, field.Unit, field.Minimum, field.Maximum));
            }
        }

        foreach (var command in manifest.OutboundCommands)
        {
            if (command.EffectiveReplyId is { Length: > 0 } replyId)
            {
                Add(new ValuePath(replyId, ValuePathType.Text, ValuePathSource.QueryReply, command.EffectiveId));
            }
        }

        foreach (var control in manifest.Ui?.Sections.SelectMany(s => s.Controls) ?? [])
        {
            Add(FromControl(control));
        }

        return paths;
    }

    /// <summary>
    /// The paths a bare <see cref="UiDefinition"/> offers: every control's id. This is how a SCPI instrument profile is read
    /// (<c>ScpiUiDefinitionBuilder.Build(profile)</c> yields a <c>{command}.reply</c> indicator per query and a
    /// <c>{command}.{parameter}</c> field per parameter), so the catalog needs no reference to the SCPI project.
    /// </summary>
    public static IReadOnlyList<ValuePath> Enumerate(UiDefinition ui)
    {
        ArgumentNullException.ThrowIfNull(ui);

        var seen = new HashSet<string>(StringComparer.Ordinal);
        return [.. ui.Sections.SelectMany(s => s.Controls).Select(FromControl).Where(p => !string.IsNullOrWhiteSpace(p.Path) && seen.Add(p.Path))];
    }

    /// <summary>The paths one response pattern publishes: its name (first capture group, or the whole match) and every named group.</summary>
    public static IReadOnlyList<ValuePath> FromPattern(ResponsePattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        var groups = ScanGroups(pattern.Match ?? string.Empty);
        var results = new List<ValuePath>();
        var example = ExampleMatch(pattern);

        var primary = groups.FirstOrDefault(g => g.Name is null);
        var wholeMatchBody = primary?.Body ?? pattern.Match ?? string.Empty;
        results.Add(new ValuePath(pattern.Name, Classify(wholeMatchBody), ValuePathSource.ResponsePattern, pattern.Name, Example: example is null ? null : example.Groups.Count > 1 ? example.Groups[1].Value : example.Value));

        foreach (var group in groups.Where(g => g.Name is not null))
        {
            results.Add(new ValuePath(group.Name!, Classify(group.Body), ValuePathSource.ResponsePattern, pattern.Name, Example: example is { } m && m.Groups[group.Name!] is { Success: true } captured ? captured.Value : null));
        }

        return results;
    }

    // The declared example run through the pattern, or null when there is none, it doesn't match, or the regex is bad.
    private static Match? ExampleMatch(ResponsePattern pattern)
    {
        if (string.IsNullOrEmpty(pattern.Example) || string.IsNullOrEmpty(pattern.Match))
        {
            return null;
        }

        try
        {
            var match = new Regex(pattern.Match, RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(250)).Match(pattern.Example);
            return match.Success ? match : null;
        }
        catch (ArgumentException)
        {
            return null;
        }
        catch (RegexMatchTimeoutException)
        {
            return null;
        }
    }

    private static ValuePath FromControl(UiControl control) => control switch
    {
        SliderControl s => new ValuePath(s.Id, ValuePathType.Number, ValuePathSource.Control, s.Label, s.Unit, s.Minimum, s.Maximum),
        NumericControl n => new ValuePath(n.Id, ValuePathType.Number, ValuePathSource.Control, n.Label, n.Unit, n.Minimum, n.Maximum),
        ToggleControl t => new ValuePath(t.Id, ValuePathType.Boolean, ValuePathSource.Control, t.Label),
        ChoiceControl c => new ValuePath(c.Id, ValuePathType.Text, ValuePathSource.Control, c.Label, Choices: c.Options),
        _ => new ValuePath(control.Id, ValuePathType.Text, ValuePathSource.Control, control.Label),
    };

    private sealed record CaptureGroup(int Start, string? Name, string Body);

    // Finds capturing groups with their body text. Non-capturing/lookaround groups ("(?:", "(?=", ...) are skipped; a
    // named group is "(?<name>" or "(?'name'" (not "(?<=" / "(?<!").
    private static List<CaptureGroup> ScanGroups(string pattern)
    {
        var groups = new List<CaptureGroup>();
        var stack = new Stack<(int Open, int BodyStart, string? Name, bool Captures)>();
        var inClass = false;

        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (c == '\\')
            {
                i++;
                continue;
            }

            if (inClass)
            {
                inClass = c != ']';
                continue;
            }

            if (c == '[')
            {
                inClass = true;
            }
            else if (c == '(')
            {
                if (i + 1 < pattern.Length && pattern[i + 1] == '?')
                {
                    var named = NamedGroupStart().Match(pattern, i);
                    stack.Push(named.Success
                        ? (i, i + named.Length, named.Groups["n"].Value, true)
                        : (i, i + 1, null, false));
                }
                else
                {
                    stack.Push((i, i + 1, null, true));
                }
            }
            else if (c == ')' && stack.Count > 0)
            {
                var (open, start, name, captures) = stack.Pop();
                if (captures)
                {
                    groups.Add(new CaptureGroup(open, name, pattern[start..i]));
                }
            }
        }

        return groups.OrderBy(g => g.Start).ToList();
    }

    private static ValuePathType Classify(string body) =>
        NumericBody().IsMatch(body) && (body.Contains(@"\d", StringComparison.Ordinal) || body.Contains("0-9", StringComparison.Ordinal))
            ? ValuePathType.Number
            : ValuePathType.Text;

    [GeneratedRegex(@"\G\(\?(?:<(?<n>[A-Za-z_]\w*)>|'(?<n>[A-Za-z_]\w*)')", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NamedGroupStart();

    [GeneratedRegex(@"^[\\d\[\]0-9.+\-eE*?{},|()\s]*$", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex NumericBody();
}
