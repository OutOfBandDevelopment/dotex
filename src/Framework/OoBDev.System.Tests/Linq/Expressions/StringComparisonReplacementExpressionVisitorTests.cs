using OoBDev.System.Linq.Expressions;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq.Expressions;

namespace OoBDev.System.Tests.Linq.Expressions;

[TestClass]
public class StringComparisonReplacementExpressionVisitorTests
{
    public required TestContext TestContext { get; set; }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Hello", "hello", true)]
    [DataRow("Hello", "HELLO", true)]
    [DataRow("Hello", "HeLlO", true)]
    public void VisitTest_EqualOperator(string input, string matched, bool expected)
    {
        var visitor = new StringComparisonReplacementExpressionVisitor();
        Expression<Func<string, bool>> expression = e => e == matched;
        var visited = visitor.Visit(expression);
        TestContext.WriteLine($"{nameof(expression)}: {expression}");
        TestContext.WriteLine($"{nameof(visited)}: {visited}");
        var result = ((LambdaExpression)visited).Compile().DynamicInvoke(input);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Hello", "hello", true)]
    [DataRow("Hello", "HELLO", true)]
    [DataRow("Hello", "HeLlO", true)]
    public void VisitTest_Equals(string input, string matched, bool expected)
    {
        var visitor = new StringComparisonReplacementExpressionVisitor();
        Expression<Func<string, bool>> expression = e => e.Equals(matched);
        var visited = visitor.Visit(expression);
        TestContext.WriteLine($"{nameof(expression)}: {expression}");
        TestContext.WriteLine($"{nameof(visited)}: {visited}");
        var result = ((LambdaExpression)visited).Compile().DynamicInvoke(input);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Hello", "el", true)]
    [DataRow("Hello", "EL", true)]
    [DataRow("Hello", "eL", true)]
    public void VisitTest_Contains(string input, string matched, bool expected)
    {
        var visitor = new StringComparisonReplacementExpressionVisitor();
        Expression<Func<string, bool>> expression = e => e.Contains(matched);
        var visited = visitor.Visit(expression);
        TestContext.WriteLine($"{nameof(expression)}: {expression}");
        TestContext.WriteLine($"{nameof(visited)}: {visited}");
        var result = ((LambdaExpression)visited).Compile().DynamicInvoke(input);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Hello", "He", true)]
    [DataRow("Hello", "HE", true)]
    [DataRow("Hello", "he", true)]
    public void VisitTest_StartsWith(string input, string matched, bool expected)
    {
        var visitor = new StringComparisonReplacementExpressionVisitor();
        Expression<Func<string, bool>> expression = e => e.StartsWith(matched);
        var visited = visitor.Visit(expression);
        TestContext.WriteLine($"{nameof(expression)}: {expression}");
        TestContext.WriteLine($"{nameof(visited)}: {visited}");
        var result = ((LambdaExpression)visited).Compile().DynamicInvoke(input);
        Assert.AreEqual(expected, result);
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("Hello", "lO", true)]
    [DataRow("Hello", "Lo", true)]
    [DataRow("Hello", "LO", true)]
    [DataRow("Hello", "lo", true)]
    public void VisitTest_EndsWith(string input, string matched, bool expected)
    {
        var visitor = new StringComparisonReplacementExpressionVisitor();
        Expression<Func<string, bool>> expression = e => e.EndsWith(matched);
        var visited = visitor.Visit(expression);
        TestContext.WriteLine($"{nameof(expression)}: {expression}");
        TestContext.WriteLine($"{nameof(visited)}: {visited}");
        var result = ((LambdaExpression)visited).Compile().DynamicInvoke(input);
        Assert.AreEqual(expected, result);
    }
}
