using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>The saved dock tree of the web front end and the pure operations on it; no UI types.</summary>
/// <param name="Version">Document version; only 1 is understood.</param>
/// <param name="Root">The root node.</param>
public sealed record DockLayout(int Version, DockNode Root)
{
    /// <summary>The smallest share a child keeps in a split.</summary>
    public const double MinRatio = 0.1;

    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    /// <summary>One pane holding every panel as a tab.</summary>
    /// <param name="panelIds">The registered panel ids.</param>
    public static DockLayout Default(IEnumerable<string> panelIds) => new(1, new DockPane([.. panelIds]));

    /// <summary>The panel ids in the tree, in order.</summary>
    public IReadOnlyList<string> PanelIds() => [.. Collect(Root)];

    /// <summary>Writes the layout as JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, _json);

    /// <summary>Reads a layout, keeps only registered panels and falls back to <see cref="Default"/> when the text is not a version 1 layout or nothing survives.</summary>
    /// <param name="json">The saved text, or null.</param>
    /// <param name="registered">The registered panel ids.</param>
    public static DockLayout Load(string? json, IReadOnlyCollection<string> registered)
    {
        ArgumentNullException.ThrowIfNull(registered);
        try
        {
            var layout = string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<DockLayout>(json, _json);
            if (layout is { Version: 1, Root: not null })
            {
                var root = Prune(layout.Root, registered);
                if (root is not null)
                {
                    return new DockLayout(1, root);
                }
            }
        }
        catch (JsonException)
        {
            // A corrupt saved layout is replaced by the default one.
        }

        return Default(registered);
    }

    /// <summary>Removes a panel; an emptied pane leaves its split and a split with one child is replaced by it.</summary>
    /// <param name="panelId">The panel to remove.</param>
    public DockLayout Close(string panelId)
    {
        var root = Prune(Root, [.. Collect(Root).Where(id => id != panelId)]);
        return root is null ? this : this with { Root = root };
    }

    /// <summary>Moves a panel to the end of the pane at <paramref name="paneIndex"/> (depth-first order) and makes it active.</summary>
    /// <param name="panelId">The panel to move.</param>
    /// <param name="paneIndex">Target pane, counted before the move.</param>
    public DockLayout Move(string panelId, int paneIndex)
    {
        var count = 0;
        var root = Map(Root, n =>
        {
            if (n is not DockPane pane)
            {
                return n;
            }

            return count++ == paneIndex
                ? new DockPane([.. pane.Tabs.Where(t => t != panelId), panelId], panelId)
                : new DockPane([.. pane.Tabs.Where(t => t != panelId)], pane.Active);
        });
        var pruned = Prune(root, [.. Collect(root)]);
        return pruned is null ? this : this with { Root = pruned };
    }

    /// <summary>Puts a panel in a new pane beside the pane at <paramref name="paneIndex"/>.</summary>
    /// <param name="panelId">The panel to move.</param>
    /// <param name="paneIndex">Target pane, depth-first order, counted before the move.</param>
    /// <param name="direction"><c>row</c> or <c>column</c>.</param>
    /// <param name="before">True to place the new pane before the target.</param>
    public DockLayout Split(string panelId, int paneIndex, string direction, bool before)
    {
        if (direction is not ("row" or "column"))
        {
            throw new ArgumentException("Direction must be row or column.", nameof(direction));
        }

        var count = 0;
        var root = Map(Root, n =>
        {
            if (n is not DockPane pane)
            {
                return n;
            }

            if (count++ != paneIndex)
            {
                return new DockPane([.. pane.Tabs.Where(t => t != panelId)], pane.Active);
            }

            var rest = new DockPane([.. pane.Tabs.Where(t => t != panelId)], pane.Active);
            DockNode added = new DockPane([panelId], panelId);
            return new DockSplit(direction, [0.5, 0.5], before ? [added, rest] : [rest, added]);
        });
        var pruned = Prune(root, [.. Collect(root)]);
        return pruned is null ? this : this with { Root = pruned };
    }

    /// <summary>The depth-first index of a pane or split in this tree (panes and splits are numbered separately), found by reference; -1 when absent.</summary>
    /// <param name="node">A node of this layout.</param>
    public int IndexOf(DockNode node)
    {
        var count = 0;
        return Find(Root, node, node is DockPane, ref count) ? count : -1;
    }

    /// <summary>Makes a tab the active one in the pane that holds it.</summary>
    /// <param name="panelId">The panel to show.</param>
    public DockLayout Activate(string panelId) =>
        this with { Root = Map(Root, n => n is DockPane p && p.Tabs.Contains(panelId) ? p with { Active = panelId } : n) };

    /// <summary>Sets the sizes of the split at <paramref name="splitIndex"/> (depth-first order), clamped to <see cref="MinRatio"/> and normalised.</summary>
    /// <param name="splitIndex">The split, depth-first order.</param>
    /// <param name="ratios">One size per child.</param>
    public DockLayout Resize(int splitIndex, IReadOnlyList<double> ratios)
    {
        var count = 0;
        var root = Map(Root, n => n is DockSplit s && count++ == splitIndex && ratios.Count == s.Children.Count ? s with { Ratios = Normalise(ratios) } : n);
        return this with { Root = root };
    }

    private static bool Find(DockNode at, DockNode target, bool panes, ref int count)
    {
        if (ReferenceEquals(at, target))
        {
            return true;
        }

        if (at is DockPane == panes)
        {
            count++;
        }

        if (at is DockSplit s)
        {
            foreach (var child in s.Children)
            {
                if (Find(child, target, panes, ref count))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static IEnumerable<string> Collect(DockNode node) => node switch
    {
        DockPane p => p.Tabs,
        DockSplit s => s.Children.SelectMany(Collect),
        _ => [],
    };

    // Pre-order: the node is mapped first; a split that the mapper left alone then has its children mapped.
    private static DockNode Map(DockNode node, Func<DockNode, DockNode> f)
    {
        var mapped = f(node);
        return mapped is DockSplit s && ReferenceEquals(mapped, node) ? s with { Children = [.. s.Children.Select(c => Map(c, f))] } : mapped;
    }

    private static IReadOnlyList<double> Normalise(IReadOnlyList<double> ratios)
    {
        var clamped = ratios.Select(r => Math.Max(MinRatio, double.IsFinite(r) ? r : MinRatio)).ToList();
        var sum = clamped.Sum();
        return [.. clamped.Select(r => r / sum)];
    }

    private static DockNode? Prune(DockNode node, IReadOnlyCollection<string> keep)
    {
        switch (node)
        {
            case DockPane p:
                var tabs = p.Tabs.Where(keep.Contains).Distinct().ToList();
                return tabs.Count == 0 ? null : new DockPane(tabs, p.Active is not null && tabs.Contains(p.Active) ? p.Active : tabs[0]);
            case DockSplit s:
                var kept = new List<(DockNode Node, double Ratio)>();
                for (var i = 0; i < s.Children.Count; i++)
                {
                    var child = Prune(s.Children[i], keep);
                    if (child is not null)
                    {
                        kept.Add((child, i < s.Ratios.Count ? s.Ratios[i] : 1.0 / s.Children.Count));
                    }
                }

                return kept.Count switch
                {
                    0 => null,
                    1 => kept[0].Node,
                    _ => new DockSplit(s.Direction is "column" ? "column" : "row", Normalise([.. kept.Select(k => k.Ratio)]), [.. kept.Select(k => k.Node)]),
                };
            default:
                return null;
        }
    }
}
