using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Tests;

/// <summary>
/// Verifies the escaped comma-join/split round trip every parameter-button join site
/// (<c>ControlPanelMode</c>/<c>ControlPanelWindow</c>) and split site (<c>ScpiControlSurface</c>/
/// <c>ManifestControlSurface</c>) now shares — see docs/bugs/resolved/024-comma-in-text-parameter.md.
/// </summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ParameterValueListTests
{
    [TestCategory(TestCategories.BugRegression)]
    [TestMethod]
    public void Join_ThenSplit_RoundTripsAValueContainingALiteralComma()
    {
        var joined = ParameterValueList.Join(["a,b", "10"]);

        Assert.AreEqual("a\\,b,10", joined);
        Assert.AreSequenceEqual(["a,b", "10"], ParameterValueList.Split(joined));
    }

    [TestMethod]
    public void Join_ThenSplit_RoundTripsAValueContainingALiteralBackslash()
    {
        var joined = ParameterValueList.Join(["a\\b", "c"]);

        Assert.AreSequenceEqual(["a\\b", "c"], ParameterValueList.Split(joined));
    }

    [TestMethod]
    public void Split_PlainCommaSeparatedString_SplitsPositionally()
    {
        Assert.AreSequenceEqual(["a", "b", "c"], ParameterValueList.Split("a,b,c"));
    }

    [TestMethod]
    public void Split_NullOrEmpty_ReturnsASingleEmptyElement()
    {
        // Matches string.Split(',')'s own behavior for an empty input (one empty element, not zero),
        // so existing "no value at index i, use the parameter's default" logic keeps working unchanged.
        Assert.AreSequenceEqual([string.Empty], ParameterValueList.Split(null));
        Assert.AreSequenceEqual([string.Empty], ParameterValueList.Split(string.Empty));
    }

    [TestMethod]
    public void Join_EmptyList_ReturnsEmptyString()
    {
        Assert.AreEqual(string.Empty, ParameterValueList.Join([]));
    }
}
