using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static OoBDev.DevTerm.Presenters.Text.Tests.TestSequence;

namespace OoBDev.DevTerm.Presenters.Text.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class DecimalPresenterTests
{
    private readonly DecimalPresenter _presenter = new();

    [TestMethod]
    public void Name_IsDecimal() => Assert.AreEqual("decimal", _presenter.Name);

    [TestMethod]
    public void Render_SpaceSeparatesEachByte() =>
        Assert.AreEqual("0 128 255", _presenter.Render(Of(0, 128, 255)).Single());

    [TestMethod]
    public void Parse_SplitsOnSpaces() =>
        CollectionAssert.AreEqual(new byte[] { 0, 128, 255 }, _presenter.Parse("0 128 255"));

    [TestMethod]
    public void RoundTrip_RenderThenParse_ReturnsOriginalBytes()
    {
        byte[] original = [1, 2, 3, 254];

        var result = _presenter.Parse(_presenter.Render(Of(original)).Single());

        CollectionAssert.AreEqual(original, result);
    }
}
