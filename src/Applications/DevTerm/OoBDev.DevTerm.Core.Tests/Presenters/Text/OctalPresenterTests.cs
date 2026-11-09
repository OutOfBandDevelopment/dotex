using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static OoBDev.DevTerm.Presenters.Text.Tests.TestSequence;

namespace OoBDev.DevTerm.Presenters.Text.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class OctalPresenterTests
{
    private readonly OctalPresenter _presenter = new();

    [TestMethod]
    public void Name_IsOctal() => Assert.AreEqual("octal", _presenter.Name);

    [TestMethod]
    public void Render_PadsEachByteToThreeDigits() =>
        Assert.AreEqual("000 007 377", _presenter.Render(Of(0, 7, 255)).Single());

    [TestMethod]
    public void RoundTrip_RenderThenParse_ReturnsOriginalBytes()
    {
        byte[] original = [0, 7, 255, 64];

        var result = _presenter.Parse(_presenter.Render(Of(original)).Single());

        CollectionAssert.AreEqual(original, result);
    }
}
