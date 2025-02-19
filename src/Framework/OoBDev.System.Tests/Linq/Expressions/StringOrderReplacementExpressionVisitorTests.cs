using OoBDev.System.Linq.Expressions;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace OoBDev.System.Tests.Linq.Expressions;

[TestClass]
public class StringOrderReplacementExpressionVisitorTests
{
    public required TestContext TestContext { get; set; }

    public record TestData
    {
        public required string Prop1 { get; init; }
        public required int Prop2 { get; init; }
        public required string Prop3 { get; init; }

    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void VisitTest_EqualOperator()
    {
        var data = new TestData[] {
            new (){ Prop1 = "upper", Prop2 = 1, Prop3="ABC" },
            new (){ Prop1 = "Upper", Prop2 = 2, Prop3="abc" },
            new (){ Prop1 = "UPPER", Prop2 = 3, Prop3="AbC" },
            new (){ Prop1 = "Upper", Prop2 = 4, Prop3="aBc" },
            new (){ Prop1 = "upper", Prop2 = 5, Prop3="ABB" },
        }.AsQueryable();

        var queryRaw = data.OrderBy(x => x.Prop1).ThenBy(x => x.Prop2).ThenBy(x => x.Prop3);
        TestContext.WriteLine("---- 1");
        TestContext.WriteLine($"{queryRaw.Expression}");

        var queryFixed = data.OrderBy(x => x.Prop1.ToUpper()).ThenBy(x => x.Prop2).ThenBy(x => x.Prop3);
        TestContext.WriteLine("---- 2");
        TestContext.WriteLine($"{queryFixed.Expression}");

        var queryCompare = data.OrderBy(x => x.Prop1, StringComparer.OrdinalIgnoreCase).ThenBy(x => x.Prop2).ThenBy(x => x.Prop3, StringComparer.OrdinalIgnoreCase);
        TestContext.WriteLine("---- 3");
        TestContext.WriteLine($"{queryCompare.Expression}");

        var visitor = new StringOrderReplacementExpressionVisitor(StringCasing.Upper);

        var visitedRaw = visitor.Visit(queryRaw.Expression);
        TestContext.WriteLine("++++ 1v");
        TestContext.WriteLine($"{visitedRaw}");

        var visitedFixed = visitor.Visit(queryFixed.Expression);
        TestContext.WriteLine("++++ 2v");
        TestContext.WriteLine($"{visitedFixed}");

        var visitedCompare = visitor.Visit(queryCompare.Expression);
        TestContext.WriteLine("++++ 3v");
        TestContext.WriteLine($"{visitedCompare}");

        var testRaw = queryRaw.ToList();
        var testFixed = queryFixed.ToList();
        var testCompare = queryCompare.ToList();

        var resultRaw = queryRaw.Provider.CreateQuery<TestData>(visitedRaw).Select(i => i.Prop1);
        var resultFixed = queryRaw.Provider.CreateQuery<TestData>(visitedFixed).Select(i => i.Prop1);
        var resultCompare = queryRaw.Provider.CreateQuery<TestData>(visitedCompare).Select(i => i.Prop1);

        TestContext.WriteLine("==== 1r");
        var checkRaw = string.Join(";", resultRaw);
        TestContext.WriteLine($"{checkRaw}");
        TestContext.WriteLine("==== 2r");
        var checkFixed = string.Join(";", resultFixed);
        TestContext.WriteLine($"{checkFixed}");
        TestContext.WriteLine("==== 3r");
        var checkCompare = string.Join(";", resultCompare);
        TestContext.WriteLine($"{checkCompare}");

        var expected = "upper;Upper;UPPER;Upper;upper";
        Assert.AreEqual(expected, checkRaw);
        Assert.AreEqual(expected, checkFixed);
        Assert.AreEqual(expected, checkCompare);
    }
}
