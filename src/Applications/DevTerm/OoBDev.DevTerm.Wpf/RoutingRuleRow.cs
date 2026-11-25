using System.Collections.Generic;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>One editable rule of the routing draft, with the hits the running router counted for it.</summary>
public sealed class RoutingRuleRow : ViewModelBase
{
    private int _hits;

    internal RoutingRuleRow(RoutingRule rule) => Rule = rule;

    /// <summary>The draft rule the row edits.</summary>
    public RoutingRule Rule { get; }

    /// <summary>The directions a rule can have.</summary>
    public static IReadOnlyList<RoutingDirection> Directions { get; } = [RoutingDirection.DeviceToBroker, RoutingDirection.BrokerToDevice];

    /// <summary>The direction of the rule.</summary>
    public RoutingDirection Direction
    {
        get => Rule.Direction;
        set
        {
            Rule.Direction = value;
            OnPropertyChanged();
        }
    }

    /// <summary>The broker topic the rule publishes to or listens on.</summary>
    public string Topic
    {
        get => Rule.Topic;
        set => Rule.Topic = value;
    }

    /// <summary>The pattern a line or message must match.</summary>
    public string Match
    {
        get => Rule.Match;
        set => Rule.Match = value;
    }

    /// <summary>The payload template for a device to broker rule.</summary>
    public string? Payload
    {
        get => Rule.Payload;
        set => Rule.Payload = string.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>The text sent to the device by a broker to device rule.</summary>
    public string? Send
    {
        get => Rule.Send;
        set => Rule.Send = string.IsNullOrEmpty(value) ? null : value;
    }

    /// <summary>True when the user is asked before the rule sends to the device.</summary>
    public bool Confirm
    {
        get => Rule.Confirm;
        set => Rule.Confirm = value;
    }

    /// <summary>How often the running router matched the rule.</summary>
    public int Hits
    {
        get => _hits;
        internal set => SetProperty(ref _hits, value);
    }
}
