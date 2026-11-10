using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Documents.Hpgl.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class HpglSceneParserTests
{
    [TestMethod]
    public void Parse_OneRun_GivesOneLineWithYPointingDownAndAMargin()
    {
        var scene = new HpglSceneParser().Parse(Encoding.ASCII.GetBytes("IN;SP2;PU0,0;PD100,0,100,50;"));

        Assert.AreEqual(1, scene.Lines.Count);
        Assert.AreEqual(2, scene.Lines[0].Pen);
        Assert.AreEqual("#3cb44b", scene.Lines[0].Color);
        Assert.AreEqual(140, scene.Width, 0.001);
        Assert.AreEqual(90, scene.Height, 0.001);
        Assert.AreEqual(new VectorPoint(20, 70), scene.Lines[0].Points[0], "HP-GL (0,0) is the bottom-left, so it lands low in the scene.");
        Assert.AreEqual(new VectorPoint(120, 20), scene.Lines[0].Points[^1]);
    }

    [TestMethod]
    public void Parse_Empty_GivesTheEmptyScene() =>
        Assert.AreSame(VectorScene.Empty, new HpglSceneParser().Parse([]));
}
