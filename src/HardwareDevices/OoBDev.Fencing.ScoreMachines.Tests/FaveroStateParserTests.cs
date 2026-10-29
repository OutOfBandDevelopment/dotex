using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Fencing.ScoreMachines.Favero;
using System;

namespace OoBDev.Fencing.ScoreMachines.Tests;

[TestClass]
public class FaveroStateParserTests
{
    private static IScoreMachineState Parse(params byte[] frame) => new FaveroStateParser().Parse(frame)!;

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_MakerExample_DecodesEveryField()
    {
        // Stage: the maker's documented example (right 6, left 12, 2:56, red + right yellow lamps, left priority, match 2, left yellow card)

        // Test
        var state = Parse(0xff, 0x06, 0x12, 0x56, 0x02, 0x14, 0x0a, 0x00, 0x38, 0x56);

        // Assert
        Assert.AreEqual(12, state.Left.Score);
        Assert.AreEqual(Lights.Touch, state.Left.Lights);
        Assert.AreEqual(Cards.Yellow, state.Left.Cards);
        Assert.IsTrue(state.Left.Priority);
        Assert.AreEqual(6, state.Right.Score);
        Assert.AreEqual(Lights.Yellow, state.Right.Lights);
        Assert.AreEqual(Cards.None, state.Right.Cards);
        Assert.IsFalse(state.Right.Priority);
        Assert.AreEqual(new TimeSpan(0, 2, 56), state.Clock);
        Assert.AreEqual(2, state.Match);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow((byte)0x01, Lights.White, Lights.None)]
    [DataRow((byte)0x02, Lights.None, Lights.White)]
    [DataRow((byte)0x03, Lights.White, Lights.White)]
    [DataRow((byte)0x04, Lights.Touch, Lights.None)]
    [DataRow((byte)0x08, Lights.None, Lights.Touch)]
    [DataRow((byte)0x10, Lights.None, Lights.Yellow)]
    [DataRow((byte)0x20, Lights.Yellow, Lights.None)]
    public void Parse_LampByte_SetsLeftAndRightLights(byte lamps, Lights left, Lights right)
    {
        var state = Parse(0xff, 0x00, 0x00, 0x00, 0x00, lamps, 0x03, 0x00, 0x38, 0x00);

        Assert.AreEqual(left, state.Left.Lights);
        Assert.AreEqual(right, state.Right.Lights);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow((byte)0x01, Cards.None, Cards.Red)]
    [DataRow((byte)0x02, Cards.Red, Cards.None)]
    [DataRow((byte)0x04, Cards.None, Cards.Yellow)]
    [DataRow((byte)0x08, Cards.Yellow, Cards.None)]
    [DataRow((byte)0x0f, Cards.Red | Cards.Yellow, Cards.Red | Cards.Yellow)]
    public void Parse_CardByte_SetsLeftAndRightCards(byte cards, Cards left, Cards right)
    {
        var state = Parse(0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, cards, 0x00);

        Assert.AreEqual(left, state.Left.Cards);
        Assert.AreEqual(right, state.Right.Cards);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow((byte)0x00, 0, false, false)]
    [DataRow((byte)0x03, 3, false, false)]
    [DataRow((byte)0x04, 0, false, true)]
    [DataRow((byte)0x09, 1, true, false)]
    public void Parse_MatchAndPriorityByte_SplitsMatchFromPriority(byte flags, int match, bool left, bool right)
    {
        var state = Parse(0xff, 0x00, 0x00, 0x00, 0x00, 0x00, flags, 0x00, 0x00, 0x00);

        Assert.AreEqual(match, state.Match);
        Assert.AreEqual(left, state.Left.Priority);
        Assert.AreEqual(right, state.Right.Priority);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_BcdDigits_ReadsTensAndUnits()
    {
        var state = Parse(0xff, 0x15, 0x09, 0x30, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00);

        Assert.AreEqual(15, state.Right.Score);
        Assert.AreEqual(9, state.Left.Score);
        Assert.AreEqual(new TimeSpan(0, 4, 30), state.Clock);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_EmptyFrame_ReturnsNull()
    {
        Assert.IsNull(new FaveroStateParser().Parse([]));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_WrongPrefix_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new FaveroStateParser().Parse(new byte[10]));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_ShortFrame_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new FaveroStateParser().Parse(new byte[] { 0xff, 0x01 }));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_NonBcdDigit_Throws()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new FaveroStateParser().Parse(new byte[] { 0xff, 0x0a, 0, 0, 0, 0, 0, 0, 0, 0 }));
    }
}
