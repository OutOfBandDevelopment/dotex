using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO.Functions;
using OoBDev.TestUtilities;
using System.Text;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class ChecksumCalculatorTests
{
    private readonly ChecksumCalculator _calculator = new();

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Xor8NmeaSentenceBodyTest()
    {
        // Sentence from the NMEA 0183 specification examples: $GPGLL,...,A*28
        var body = Encoding.ASCII.GetBytes("GPGLL,5300.97914,N,00259.98174,E,125926,A");

        Assert.AreEqual((byte)0x28, _calculator.Xor8(body));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Xor8EmptyIsZeroTest() =>
        Assert.AreEqual((byte)0, _calculator.Xor8([]));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WordSumComplement16SumsLittleEndianWordsTest() =>
        // words 0x0001 + 0x0002 = 3
        Assert.AreEqual((ushort)(0xFFFF - 3), _calculator.WordSumComplement16([0x01, 0x00, 0x02, 0x00]));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WordSumComplement16ReadsLowByteFirstTest() =>
        // word 0x0100, not 0x0001
        Assert.AreEqual((ushort)(0xFFFF - 0x0100), _calculator.WordSumComplement16([0x00, 0x01]));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WordSumComplement16WrapsAtFfffTest() =>
        // 0xFFFF % 0xFFFF == 0
        Assert.AreEqual((ushort)0xFFFF, _calculator.WordSumComplement16([0xFF, 0xFF]));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WordSumComplement16EmptyTest() =>
        Assert.AreEqual((ushort)0xFFFF, _calculator.WordSumComplement16([]));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WordSumComplement16IgnoresTrailingOddByteTest() =>
        Assert.AreEqual(_calculator.WordSumComplement16([0x01, 0x00]), _calculator.WordSumComplement16([0x01, 0x00, 0x7F]));
}
