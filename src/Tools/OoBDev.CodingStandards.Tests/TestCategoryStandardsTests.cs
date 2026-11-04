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
    /// <summary>The categories the <c>testing</c> rule recognizes. Add one here in the same change that starts using it.</summary>
    private static readonly string[] _knownCategories = ["Unit", "Simulate", "Integration", "DevLocal", "LiveIntegration", "Hardware"];

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
            if (effective.Count == 0)
            {
                problems.Add($"{type.File}: {type.Name} has no [TestCategory]");
                continue;
            }

            problems.AddRange(effective.Where(c => !_knownCategories.Contains(c, StringComparer.Ordinal)).Distinct()
                .Select(c => $"{type.File}: {type.Name} uses the unrecognized category '{c}'"));
        }

        // Assert
        Assert.IsEmpty(problems, $"Every [TestClass] needs a [TestCategory] from {{{string.Join(", ", _knownCategories)}}}:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    [TestMethod]
    public void EveryTestMethod_HasExactlyOneEffectiveCategory()
    {
        // Stage / Test
        var problems = new List<string>();
        foreach (var type in TestClasses().Where(t => !_knownGaps.Contains(t.Name, StringComparer.Ordinal)))
        {
            problems.AddRange(type.Methods
                .Where(m => type.ClassCategories.Concat(m.Categories).Distinct(StringComparer.Ordinal).Count() > 1)
                .Select(m => $"{type.File}: {type.Name}.{m.Name} has more than one category"));
        }

        // Assert
        Assert.IsEmpty(problems, $"A test carries one category:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }
}
