using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.LlmCodeGen.Cli.Tests;

[TestClass]
public class LlmCodeGenOptionsTests
{
    private static List<ValidationResult> Validate(LlmCodeGenOptions options)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(options, new ValidationContext(options), results, validateAllProperties: true);
        return results;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Validate_NothingSet_ReportsEveryRequiredValue()
    {
        // Test
        var results = Validate(new LlmCodeGenOptions());

        // Assert
        var members = results.SelectMany(r => r.MemberNames).ToList();
        CollectionAssert.AreEquivalent(
            new[] { nameof(LlmCodeGenOptions.InputPath), nameof(LlmCodeGenOptions.OutputPath), nameof(LlmCodeGenOptions.Template), nameof(LlmCodeGenOptions.Model) },
            members);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Validate_RequiredValuesSet_PassesWithDefaults()
    {
        // Stage
        var options = new LlmCodeGenOptions { InputPath = "in", OutputPath = "out", Template = "documentation", Model = "phi3" };

        // Test
        var results = Validate(options);

        // Assert
        Assert.AreEqual(0, results.Count);
        Assert.AreEqual("ollama", options.Provider);
        Assert.AreEqual("*.*", options.SearchPattern);
        Assert.IsFalse(options.Overwrite);
        Assert.IsTrue(options.ExtractFiles);
    }
}
