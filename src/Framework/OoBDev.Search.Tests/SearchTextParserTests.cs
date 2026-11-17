using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;

namespace OoBDev.Search.Tests;

[TestClass]
public class SearchTextParserTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_PlainWords_StaysFreeText()
    {
        var request = SearchTextParser.Parse("red  barn   sunset");

        Assert.AreEqual("red barn sunset", request.Text);
        Assert.AreEqual(0, request.Filters.Count);
        Assert.IsNull(request.Anchor);
        Assert.AreEqual(20, request.PageSize);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_FieldAndNegation_BecomeFilters()
    {
        var request = SearchTextParser.Parse("ext:pdf -tag:draft invoice");

        Assert.AreEqual("invoice", request.Text);
        Assert.AreEqual(new FieldFilter("ext", FilterComparator.Equals, "pdf"), request.Filters[0]);
        Assert.AreEqual(new FieldFilter("tag", FilterComparator.Equals, "draft", true), request.Filters[1]);
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("size:>10", FilterComparator.Greater, "10")]
    [DataRow("size:>=10", FilterComparator.GreaterOrEqual, "10")]
    [DataRow("size:<10", FilterComparator.Less, "10")]
    [DataRow("size:<=10", FilterComparator.LessOrEqual, "10")]
    [DataRow("name:~logo", FilterComparator.Contains, "logo")]
    public void Parse_ComparatorPrefix_MapsComparator(string text, FilterComparator comparator, string value)
    {
        var filter = SearchTextParser.Parse(text).Filters[0];

        Assert.AreEqual(comparator, filter.Comparator);
        Assert.AreEqual(value, filter.Value);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_QuotedValue_KeepsSpaces()
    {
        var request = SearchTextParser.Parse("tag:\"two words\" \"exact phrase\"");

        Assert.AreEqual("two words", request.Filters[0].Value);
        Assert.AreEqual("\"exact phrase\"", request.Text);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_ReservedNames_SetRequestParts()
    {
        var request = SearchTextParser.Parse("like:item-1 near:beach sort:-date,name top:5 after:abc");

        Assert.AreEqual("item-1", request.Anchor?.LikeItemId);
        Assert.AreEqual("beach", request.Anchor?.NearText);
        CollectionAssert.AreEqual(new[] { "-date", "name" }, request.Sort.ToArray());
        Assert.AreEqual(5, request.PageSize);
        Assert.AreEqual("abc", request.Cursor);
        Assert.AreEqual(0, request.Filters.Count);
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("top:0")]
    [DataRow("top:many")]
    public void Parse_BadTop_Throws(string text) =>
        Assert.ThrowsExactly<FormatException>(() => SearchTextParser.Parse(text));

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("http://x")]
    [DataRow("trailing:")]
    [DataRow(":lead")]
    [DataRow("a b:c")]
    public void Parse_NotAFilter_IsWordOrFilterAsExpected(string text)
    {
        var request = SearchTextParser.Parse(text);

        Assert.IsTrue(request.Text.Length > 0 || request.Filters.Count > 0);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_NegatedReservedName_IsFilter()
    {
        var request = SearchTextParser.Parse("-top:5");

        Assert.AreEqual("top", request.Filters[0].Name);
        Assert.IsTrue(request.Filters[0].Negated);
    }
}
