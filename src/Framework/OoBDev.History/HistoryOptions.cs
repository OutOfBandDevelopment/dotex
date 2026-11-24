using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.History;

public sealed class HistoryOptions
{
    public TimeSpan RawRetention { get; set; } = TimeSpan.FromDays(1);

    public IList<HistoryTier> Tiers { get; set; } =
    [
        new HistoryTier("1m", TimeSpan.FromMinutes(1), TimeSpan.FromDays(7)),
        new HistoryTier("1h", TimeSpan.FromHours(1), TimeSpan.FromDays(90)),
        new HistoryTier("1d", TimeSpan.FromDays(1), TimeSpan.FromDays(365 * 5)),
    ];

    public HistoryTier GetTier(string name) =>
        Tiers.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"Unknown history tier '{name}'.", nameof(name));
}
