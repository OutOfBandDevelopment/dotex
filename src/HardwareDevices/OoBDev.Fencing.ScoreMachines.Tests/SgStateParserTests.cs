using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Fencing.ScoreMachines.SaintGeorge;
using System;
using System.Text;

namespace OoBDev.Fencing.ScoreMachines.Tests;

/// <summary>Frames follow the layouts captured in the BinaryDataDecoders notes; none was recorded from a live machine for these tests.</summary>
[TestClass]
public class SgStateParserTests
{
    private static byte[] Frame(string text) => Encoding.ASCII.GetBytes(text);

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_ScoreAndCardsFrame_SetsBothSides()
    {
        // right 011, left 005; green card digit 2 (red), red card digit 1 (yellow)
        var parser = new SgStateParser();

        var state = parser.Parse(Frame("\u0001\u0013ST\u0002011:005\u000202\u000201\u0004"))!;

        Assert.AreEqual(5, state.Left.Score);
        Assert.AreEqual(Cards.Yellow, state.Left.Cards);
        Assert.AreEqual(11, state.Right.Score);
        Assert.AreEqual(Cards.Red, state.Right.Cards);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_LightsFrame_SetsTouchAndOffTarget()
    {
        var state = new SgStateParser().Parse(Frame("\u0001\u0013LR1G0W0w1\u0004"))!;

        Assert.AreEqual(Lights.Touch, state.Left.Lights);
        Assert.AreEqual(Lights.White, state.Right.Lights);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_LongClockFrame_ReadsMinutesAndSeconds()
    {
        var state = new SgStateParser().Parse(Frame("\u0001\u0013R_F$\u0002\u00100000__:02:59.___\u0004"))!;

        Assert.AreEqual(new TimeSpan(0, 2, 59), state.Clock);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_ShortScoreFrame_ReadsRightThenLeft()
    {
        var state = new SgStateParser().Parse(Frame("\u0001\u0013S01:06\u0004"))!;

        Assert.AreEqual(1, state.Right.Score);
        Assert.AreEqual(6, state.Left.Score);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_ShortClockFrame_ReadsMinutesAndSeconds()
    {
        var state = new SgStateParser().Parse(Frame("\u0001\u0013T02:16\u0004"))!;

        Assert.AreEqual(new TimeSpan(0, 2, 16), state.Clock);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_PriorityFrame_KeepsScoreAndClock()
    {
        // The source reads the priority from the 'P' byte, which never carries it, so the flag stays off; see the parser's comment.
        var parser = new SgStateParser();
        parser.Parse(Frame("\u0001\u0013S01:06\u0004"));

        var state = parser.Parse(Frame("\u0001\u0012P?AA8G\u0004"))!;

        Assert.AreEqual(6, state.Left.Score);
        Assert.IsFalse(state.Left.Priority);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_SeveralFrames_AccumulatesFactsFromEach()
    {
        var parser = new SgStateParser();
        parser.Parse(Frame("\u0001\u0013S01:06\u0004"));
        parser.Parse(Frame("\u0001\u0013LR1G0W0w0\u0004"));

        var state = parser.Parse(Frame("\u0001\u0013T02:16\u0004"))!;

        Assert.AreEqual(6, state.Left.Score);
        Assert.AreEqual(1, state.Right.Score);
        Assert.AreEqual(Lights.Touch, state.Left.Lights);
        Assert.AreEqual(new TimeSpan(0, 2, 16), state.Clock);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_UnknownOrUnframedInput_ReturnsLastState()
    {
        var parser = new SgStateParser();
        var known = parser.Parse(Frame("\u0001\u0013S01:06\u0004"));

        Assert.AreSame(known, parser.Parse([]));
        Assert.AreSame(known, parser.Parse(Frame("S01:06")));
        Assert.AreSame(known, parser.Parse(Frame("\u0001\u0013Z99\u0004")));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_FirstFrameUnknown_ReturnsEmptyState()
    {
        Assert.AreSame(ScoreMachineState.Empty, new SgStateParser().Parse(Frame("\u0001\u0013Z99\u0004")));
    }
}
