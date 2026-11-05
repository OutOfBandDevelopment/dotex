using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.CodingStandards.Tests;

/// <summary>
/// Enforces "every test carries one category" (rule <c>testing</c>). A missing or misspelled category does not fail a build:
/// MSTest just leaves the test out of every <c>--filter TestCategory=...</c>, so nothing else would catch it.
/// </summary>
[TestCategory("Unit")]
[TestClass]
public sealed class TestCategoryStandardsTests
{
    /// <summary>The tiers: every test carries exactly one.</summary>
    private static readonly string[] _tiers = ["Unit", "Simulate", "Integration", "DevLocal", "LiveIntegration", "Hardware"];

    /// <summary>Secondary categories (transport, feature, device): optional, any number, beside the tier. Add one here in the same change that starts using it.</summary>
    private static readonly string[] _secondaryCategories =
    [
        "Serial", "Tcp", "Hid", "Usbtmc", "Ble", "Loopback", "Rfc2217", "Vxi11", "Mqtt", "Brokers", "Web", "Scpi",
        "Logging", "BugRegression",
        "Tektronix_2230", "Tektronix_Tds2024", "Korad_Ka3005p", "Korad_Ka6003p", "Hp_34401a", "Rigol_Ds1102e", "Rigol_Dm3058e",
        "Rigol_Dg1022", "Rigol_Dg1062z", "Velleman_K8055", "Kuando_Busylight", "Radex_One", "Zoom_H4n", "DerEe_De5000",
        "Delorme_EarthmateBt20", "Aeotec_ZStickS2",
    ];

    private static readonly string[] _knownCategories = [.. _tiers, .. _secondaryCategories];

    /// <summary>Classes that predate this check. Shrink this list; never add to it.</summary>
    private static readonly string[] _knownGaps = [];

    private static List<TestClassInfo> TestClasses() => [.. TestSourceScanner.Scan(RepositoryRoot.Find(), "src")];

    [TestMethod]
    public void Scanner_FindsTheRepositoryTests()
    {
        // Stage / Test
        var classes = TestClasses();

        // Assert
        Assert.IsGreaterThan(200, classes.Count, "The scan should find the test classes of the whole repository.");
    }

    [TestMethod]
    public void EveryTestClass_DeclaresARecognizedCategory()
    {
        // Stage / Test
        var problems = new List<string>();
        foreach (var type in TestClasses().Where(t => !_knownGaps.Contains(t.Name, StringComparer.Ordinal)))
        {
            var effective = type.ClassCategories.Concat(type.Methods.SelectMany(m => m.Categories)).ToList();
            if (!effective.Any(c => _tiers.Contains(c, StringComparer.Ordinal)))
            {
                problems.Add($"{type.File}: {type.Name} has no tier category");
                continue;
            }

            problems.AddRange(effective.Where(c => !_knownCategories.Contains(c, StringComparer.Ordinal)).Distinct()
                .Select(c => $"{type.File}: {type.Name} uses the unrecognized category '{c}'"));
        }

        // Assert
        Assert.IsEmpty(problems, $"Every [TestClass] needs a tier from {{{string.Join(", ", _tiers)}}} and may add secondary categories:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    [TestMethod]
    public void EveryTestMethod_HasExactlyOneEffectiveCategory()
    {
        // Stage / Test
        var problems = new List<string>();
        foreach (var type in TestClasses().Where(t => !_knownGaps.Contains(t.Name, StringComparer.Ordinal)))
        {
            problems.AddRange(type.Methods
                .Where(m => type.ClassCategories.Concat(m.Categories).Where(c => _tiers.Contains(c, StringComparer.Ordinal)).Distinct(StringComparer.Ordinal).Count() != 1)
                .Select(m => $"{type.File}: {type.Name}.{m.Name} does not have exactly one tier category"));
        }

        // Assert
        Assert.IsEmpty(problems, $"A test carries exactly one tier:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}
