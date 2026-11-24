using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Documents.Hpgl.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class PostScriptAndPclSceneParserTests
{
    public TestContext TestContext { get; set; } = null!;

    private static byte[] Bytes(string text) => Encoding.Latin1.GetBytes(text);

    [TestMethod]
    public void PostScript_LineStroke_GivesOneLineWithYPointingDown()
    {
        var scene = new PostScriptSceneParser().Parse(Bytes("%!PS\nnewpath 0 0 moveto 100 0 lineto 100 50 lineto stroke showpage"));
        TestContext.WriteLine($"{scene.Width}x{scene.Height}, {scene.Lines.Count} line(s)");

        Assert.AreEqual(1, scene.Lines.Count);
        Assert.AreEqual(140, scene.Width, 0.001);
        Assert.AreEqual(90, scene.Height, 0.001);
        Assert.AreEqual(new VectorPoint(20, 70), scene.Lines[0].Points[0]);
        Assert.AreEqual(new VectorPoint(120, 20), scene.Lines[0].Points[^1]);
        Assert.AreEqual("#000000", scene.Lines[0].Color);
    }

    [TestMethod]
    public void PostScript_ColorMatrixAndRelativeOperators_AreApplied()
    {
        var scene = new PostScriptSceneParser().Parse(Bytes("1 0 0 setrgbcolor gsave 10 10 translate 2 2 scale newpath 0 0 moveto 5 0 rlineto 0 5 rlineto closepath stroke grestore"));

        Assert.AreEqual(1, scene.Lines.Count);
        Assert.AreEqual("#ff0000", scene.Lines[0].Color);
        Assert.AreEqual(4, scene.Lines[0].Points.Count, "two segments and the closing point");
        Assert.AreEqual(10, scene.Width - 40, 0.001, "5 units become 10 after the scale");
    }

    [TestMethod]
    public void PostScript_TextProceduresAndComments_AreSkipped()
    {
        var scene = new PostScriptSceneParser().Parse(Bytes("% a line (not drawn) 1 1 moveto\n(text with (nested) parens) show /f { 9 9 moveto 99 99 lineto stroke } def 0 0 moveto 10 10 lineto stroke"));

        Assert.AreEqual(1, scene.Lines.Count);
        Assert.AreEqual(2, scene.Lines[0].Points.Count);
    }

    [TestMethod]
    public void PostScript_ArcCurveAndRectangle_AreFlattened()
    {
        var scene = new PostScriptSceneParser().Parse(Bytes("newpath 0 0 10 0 360 arc stroke 0 0 moveto 1 5 9 5 10 0 curveto stroke 0 0 5 5 rectstroke"));

        Assert.AreEqual(3, scene.Lines.Count);
        Assert.IsTrue(scene.Lines[0].Points.Count > 30, "a circle in 10 degree steps");
        Assert.AreEqual(13, scene.Lines[1].Points.Count);
        Assert.AreEqual(5, scene.Lines[2].Points.Count);
    }

    [TestMethod]
    public void PostScript_NothingDrawnOrGarbage_GivesTheEmptyScene()
    {
        Assert.AreSame(VectorScene.Empty, new PostScriptSceneParser().Parse([]));
        Assert.AreSame(VectorScene.Empty, new PostScriptSceneParser().Parse(Bytes("lineto moveto stroke )) } { (")));
    }

    [TestMethod]
    public void Pcl_Hpgl2Segment_IsParsedAndTheRestIgnored()
    {
        var pcl = "\u001bE\u001b&l0O Some page text \u001b%0BIN;SP1;PU0,0;PD100,0,100,50;\u001b%0A more text \u001b%-12345X";
        var scene = new PclSceneParser().Parse(Bytes(pcl));

        Assert.AreEqual(1, scene.Lines.Count);
        Assert.AreEqual(140, scene.Width, 0.001);
    }

    [TestMethod]
    public void Pcl_TwoSegmentsAndAnUnterminatedOne_AreJoined()
    {
        var pcl = "\u001b%1BIN;PU0,0;PD10,10;\u001b%0Atext\u001b%0BPU20,0;PD30,10;\u001b%0Atext\u001b%1BPU40,0;PD50,10;";
        var scene = new PclSceneParser().Parse(Bytes(pcl));

        Assert.AreEqual(3, scene.Lines.Count);
    }

    [TestMethod]
    public void Pcl_NoHpgl2_GivesTheEmptyScene()
    {
        Assert.AreSame(VectorScene.Empty, new PclSceneParser().Parse(Bytes("\u001bE plain text \u001b*t300R")));
        Assert.AreSame(VectorScene.Empty, new PclSceneParser().Parse([]));
    }
}
