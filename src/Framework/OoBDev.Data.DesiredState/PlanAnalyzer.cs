using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Reads the statements of a plan and flags the hazards that no diff tool reports on its own (bake-off S5 and S6): drops and column type changes.
/// </summary>
public static partial class PlanAnalyzer
{
    /// <summary>Splits dry-run output into executable statements and the drops the tool skipped (<c>-- Skipped:</c> lines); other comments are ignored.</summary>
    public static (IReadOnlyList<string> Statements, IReadOnlyList<string> Skipped) Parse(string output)
    {
        ArgumentNullException.ThrowIfNull(output);
        var statements = new List<string>();
        var skipped = new List<string>();
        var current = new List<string>();
        foreach (var raw in output.Split('\n'))
        {
            var line = raw.TrimEnd('\r').TrimEnd();
            var trimmed = line.TrimStart();
            if (current.Count == 0)
            {
                if (trimmed.Length == 0)
                {
                    continue;
                }

                if (trimmed.StartsWith("--", StringComparison.Ordinal))
                {
                    const string marker = "-- Skipped:";
                    if (trimmed.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                    {
                        skipped.Add(trimmed[marker.Length..].Trim());
                    }

                    continue;
                }
            }

            current.Add(line);
            if (trimmed.EndsWith(';'))
            {
                statements.Add(string.Join('\n', current).Trim());
                current.Clear();
            }
        }

        if (current.Count > 0)
        {
            statements.Add(string.Join('\n', current).Trim());
        }

        return (statements, skipped);
    }

    /// <summary>Flags every statement that drops an object or changes a column type.</summary>
    public static IReadOnlyList<Hazard> Analyze(IEnumerable<string> statements)
    {
        ArgumentNullException.ThrowIfNull(statements);
        var hazards = new List<Hazard>();
        foreach (var statement in statements)
        {
            if (DataLossPattern().IsMatch(statement))
            {
                hazards.Add(new Hazard(HazardKind.DataLoss, statement, "removes a table, column, schema or database and the data in it"));
            }
            else if (DropPattern().IsMatch(statement))
            {
                hazards.Add(new Hazard(HazardKind.Drop, statement, "removes an object"));
            }

            if (TypeChangePattern().IsMatch(statement))
            {
                hazards.Add(new Hazard(HazardKind.TypeChange, statement, "changes a column type; the new type may be narrower and the table may be rewritten"));
            }
        }

        return hazards;
    }

    [GeneratedRegex(@"\b(DROP\s+(TABLE|COLUMN|SCHEMA|DATABASE)|TRUNCATE\s+TABLE)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, 200)]
    private static partial Regex DataLossPattern();

    [GeneratedRegex(@"\bDROP\s+(INDEX|CONSTRAINT|VIEW|FUNCTION|TRIGGER|PROCEDURE|SEQUENCE|TYPE|EXTENSION)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, 200)]
    private static partial Regex DropPattern();

    [GeneratedRegex(@"\bALTER\s+COLUMN\s+\S+\s+(SET\s+DATA\s+)?TYPE\b|\bALTER\s+COLUMN\s+\S+\s+(?!SET\b|DROP\b|ADD\b)\w+|\bMODIFY\s+(COLUMN\s+)?\S+\s+\w+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, 200)]
    private static partial Regex TypeChangePattern();
}
