using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using static OoBDev.DevTerm.Presenters.Text.Tests.TestSequence;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Presenters.Text.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class Utf8PresenterTests
{
    private readonly Utf8Presenter _presenter = new();

    [TestMethod]
    public void Name_IsUtf8() => Assert.AreEqual("utf8", _presenter.Name);

    [TestMethod]
    public void RoundTrip_ParseThenRender_ReturnsOriginalText_IncludingMultiByteCharacters()
    {
        const string original = "café — 日本語";

        var result = _presenter.Render(Of(_presenter.Parse(original))).Single();

        Assert.AreEqual(original, result);
    }

    [TestCategory(TestCategories.BugRegression)]
    [TestMethod]
    public void Render_MultiByteCharacterSplitAcrossTwoReads_DecodesCorrectlyOnceComplete()
    {
        var bytes = Encoding.UTF8.GetBytes("é");
        Assert.AreEqual(2, bytes.Length);

        var first = _presenter.Render(Of(bytes[..1]));
        var second = _presenter.Render(Of(bytes[1..]));

        Assert.IsEmpty(first);
        Assert.AreEqual("é", second.Single());
    }

    [TestMethod]
    public void Restore_SnapshotTakenMidCharacter_CompletesTheCharacterOnAFreshPresenter()
    {
        var bytes = Encoding.UTF8.GetBytes("日");
        _presenter.Render(Of(bytes[..2]));
        var snapshot = _presenter.Snapshot();
        var restored = new Utf8Presenter();

        restored.Restore(snapshot);
        var result = restored.Render(Of(bytes[2..]));

        Assert.AreEqual("日", result.Single());
    }

    [TestMethod]
    public void Restore_SnapshotTakenOnACharacterBoundary_StartsClean()
    {
        _presenter.Render(Of(Encoding.UTF8.GetBytes("aé")));
        var snapshot = _presenter.Snapshot();
        var restored = new Utf8Presenter();

        restored.Restore(snapshot);

        Assert.AreEqual("b", restored.Render(Of("b"u8.ToArray())).Single());
    }

    [TestMethod]
    public void Restore_LaterRendersAfterTheSnapshot_DoNotChangeIt()
    {
        var bytes = Encoding.UTF8.GetBytes("é");
        _presenter.Render(Of(bytes[..1]));
        var snapshot = _presenter.Snapshot();
        _presenter.Render(Of(bytes[1..]));

        _presenter.Restore(snapshot);

        Assert.AreEqual("é", _presenter.Render(Of(bytes[1..])).Single());
    }

    [TestMethod]
    public void Restore_ForeignSnapshot_Throws() =>
        Assert.ThrowsExactly<global::System.ArgumentException>(() => _presenter.Restore("x"));
}
