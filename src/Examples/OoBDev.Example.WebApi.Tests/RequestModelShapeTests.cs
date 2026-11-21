using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Models;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class RequestModelShapeTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void PositionalRecords_HaveNoValidationAttributesOnProperties()
    {
        // Stage: MVC throws (ThrowIfRecordTypeHasValidationOnProperties) when a positional record carries [property: Required] and the like
        var models = typeof(GeocodeRequestModel).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(GeocodeRequestModel).Namespace && t.GetMethod("<Clone>$") is not null)
            .ToArray();
        TestContext.WriteLine($"Checked {models.Length} record models");

        // Test
        var offenders = models
            .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance), (t, p) => (t, p))
            .Where(x => x.p.GetCustomAttributes<ValidationAttribute>(true).Any())
            .Select(x => $"{x.t.Name}.{x.p.Name}")
            .ToArray();

        // Assert
        Assert.IsGreaterThan(0, models.Length);
        Assert.AreEqual(0, offenders.Length, string.Join(", ", offenders));
    }
}
