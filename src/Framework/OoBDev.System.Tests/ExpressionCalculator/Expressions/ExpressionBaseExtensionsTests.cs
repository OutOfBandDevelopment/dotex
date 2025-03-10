﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.ExpressionCalculator.Expressions;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.System.Tests.ExpressionCalculator.Expressions;

[TestClass]
public class ExpressionBaseExtensionsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(ExpressionBaseExtensions), Member = nameof(ExpressionBaseExtensions.Evaluate))]
    public void ParseAndEvaluateTest()
    {
        var input = "A+B";
        var parsed = input.ParseAsExpression<double>();

        var testValues = new[]
        {
            ("A", 2.1),
            ("B", 3.4)
        };

        var variables = string.Join(", ", testValues.Select(kvp => (Name: kvp.Item1, Value: kvp.Item2)));
        var result = parsed.Evaluate(testValues);

        TestContext.WriteLine($"Input: {input}");
        TestContext.WriteLine($"As Parsed: {parsed}");
        TestContext.WriteLine($"Variables: {variables}");
        TestContext.WriteLine($"Result: {result}");

        Assert.AreEqual(5.5, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(ExpressionBaseExtensions), Member = nameof(ExpressionBaseExtensions.PreEvaluate))]
    public void ParseAndPreEvaluateTest()
    {
        var input = "A+B";
        var parsed = input.ParseAsExpression<double>();

        var testValues = new[]
        {
            ("A", 2.1),
            ("B", 3.4)
        };

        var variables = string.Join(", ", testValues.Select(kvp => (Name: kvp.Item1, Value: kvp.Item2)));
        var result = parsed.PreEvaluate(testValues);

        TestContext.WriteLine($"Input: {input}");
        TestContext.WriteLine($"As Parsed: {parsed}");
        TestContext.WriteLine($"Variables: {variables}");
        TestContext.WriteLine($"Result: {result}");

        Assert.AreEqual("2.1 + 3.4", result.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(ExpressionBaseExtensions), Member = nameof(ExpressionBaseExtensions.ReplaceVariables))]
    public void ParseAndReplaceVariablesTest()
    {
        var input = "A+B";
        var parsed = input.ParseAsExpression<double>();

        var testValues = new[]
        {
            ("A", "X"),
            ("B", "Y")
        };

        var variables = string.Join(", ", testValues.Select(kvp => (Name: kvp.Item1, Value: kvp.Item2)));
        var result = parsed.ReplaceVariables(testValues);

        TestContext.WriteLine($"Input: {input}");
        TestContext.WriteLine($"As Parsed: {parsed}");
        TestContext.WriteLine($"Variables: {variables}");
        TestContext.WriteLine($"Result: {result}");

        Assert.AreEqual("X + Y", result.ToString());
    }
}
