using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.DevTerm;

/// <summary>
/// One mapping rule, serializable as JSON (see <see cref="RoutingRuleSet"/>). <see cref="Match"/> is a regex
/// over the device line (device to broker) or the broker payload text (broker to device); its named groups
/// are available as <c>${name}</c> in <see cref="Payload"/>, <see cref="Topic"/> and <see cref="Send"/>.
/// </summary>
public sealed class RoutingRule
{
    private const string Placeholder = @"\$\{(?<name>\w+)\}";
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

    private Regex? _compiled;

    public RoutingDirection Direction { get; set; }

    /// <summary>Device to broker: the topic to publish to. Broker to device: the topic that triggers the rule (exact match, or a trailing <c>#</c> wildcard).</summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>A regex over the message text. Empty matches everything.</summary>
    public string Match { get; set; } = string.Empty;

    /// <summary>Device to broker only: the published payload. Defaults to the whole line.</summary>
    public string? Payload { get; set; }

    /// <summary>Broker to device only: the text sent to the device (the router appends the line terminator).</summary>
    public string? Send { get; set; }

    /// <summary>Broker to device only: ask before the first matching message reaches the device (see <see cref="MessageRouter.Confirm"/>).</summary>
    public bool Confirm { get; set; }

    /// <summary>Checks the rule can run: the regex compiles and every <c>${name}</c> names a group in <see cref="Match"/>. Returns the problem, or null.</summary>
    public string? Validate()
    {
        Regex regex;
        try
        {
            regex = new Regex(Match, RegexOptions.CultureInvariant, RegexTimeout);
        }
        catch (ArgumentException ex)
        {
            return $"Match is not a valid regex: {ex.Message}";
        }

        if (string.IsNullOrWhiteSpace(Topic))
        {
            return "Topic is required.";
        }

        if (Direction == RoutingDirection.BrokerToDevice && string.IsNullOrEmpty(Send))
        {
            return "Send is required for a broker-to-device rule.";
        }

        var names = regex.GetGroupNames();
        foreach (var template in new[] { Topic, Payload, Send })
        {
            if (template is null)
            {
                continue;
            }

            foreach (Match m in Regex.Matches(template, Placeholder, RegexOptions.CultureInvariant, RegexTimeout))
            {
                if (!names.Contains(m.Groups["name"].Value))
                {
                    return $"${{{m.Groups["name"].Value}}} does not name a group in Match.";
                }
            }
        }

        return null;
    }

    /// <summary>Runs the rule against sample text without side effects: null when it does not match, else the topic and the payload (or send text) it would produce.</summary>
    public RuleTestResult? Test(string sample)
    {
        var match = Compiled.Match(sample);
        if (!match.Success)
        {
            return null;
        }

        return Direction == RoutingDirection.DeviceToBroker
            ? new RuleTestResult(Expand(Topic, match), Payload is null ? sample : Expand(Payload, match))
            : new RuleTestResult(Topic, Send is null ? string.Empty : Expand(Send, match));
    }

    internal Regex Compiled => _compiled ??= new Regex(Match, RegexOptions.CultureInvariant, RegexTimeout);

    internal bool TopicMatches(string topic) =>
        Topic.EndsWith('#') ? topic.StartsWith(Topic[..^1], StringComparison.Ordinal) : string.Equals(Topic, topic, StringComparison.Ordinal);

    internal static string Expand(string template, Match match) =>
        Regex.Replace(template, Placeholder, m => match.Groups[m.Groups["name"].Value] is { Success: true } g ? g.Value : string.Empty, RegexOptions.CultureInvariant, RegexTimeout);
}
