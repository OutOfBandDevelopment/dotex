using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Numerics;
using OoBDev.TestUtilities;
using System;
using System.Globalization;

namespace OoBDev.System.Tests.Numerics;

[TestClass]
public class ByteNumberTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(1d, "1b")]
    [DataRow(1000d, "1000b")]
    [DataRow(1001d, "1001b")]
    [DataRow(1024d, "1kb")]
    [DataRow(1026d, "1.002kb")]
    [DataRow(1001000d, "977.539kb")]
    [DataRow(0.110, "112.64mb")]
    [DataRow(0.110123, "112.766mb")]
    public void ToString_Double_UsesByteNotation(double input, string expected)
    {
        ByteNumber number = input;
        Assert.AreEqual(expected, number.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("1b", 1d)]
    [DataRow("1000b", 1000d)]
    [DataRow("1kb", 1024d)]
    [DataRow("1.002kb", 1026.048d)]
    [DataRow("977.539kb", 1000999.936d)]
    [DataRow("112.64mb", 0.110d)]
    [DataRow("112.766mb", 0.110123046875d)]
    public void TryParse_ByteNotation_ReturnsValue(string input, double expected)
    {
        Assert.IsTrue(ByteNumber.TryParse(input, out var value));
        Assert.AreEqual(expected, (double)value!);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("b")]
    [DataRow("12")]
    [DataRow("xkb")]
    [DataRow("xb")]
    public void TryParse_Invalid_ReturnsFalse(string? input)
    {
        Assert.IsFalse(ByteNumber.TryParse(input, out var value));
        Assert.IsNull(value);
    }
}

[TestClass]
public class ByteNotationFormatterTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(1d, "1b")]
    [DataRow(1024d, "1kb")]
    [DataRow(1001000d, "977.539kb")]
    [DataRow(0.110, "112.64mb")]
    public void Format_Value_UsesByteNotation(double input, string expected) =>
        Assert.AreEqual(expected, string.Format(new ByteNotationFormatter(), "{0}", input));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Format_Zero_DoesNotScale() =>
        Assert.AreEqual("0b", string.Format(new ByteNotationFormatter(), "{0}", 0d));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetFormat_OtherType_ReturnsNull() =>
        Assert.IsNull(new ByteNotationFormatter().GetFormat(typeof(string)));
}

[TestClass]
public class EngineeringNumberTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(1d, "1")]
    [DataRow(1000d, "1k")]
    [DataRow(1001d, "1.001k")]
    [DataRow(1001000d, "1.001M")]
    [DataRow(0.110, "110m")]
    [DataRow(0.110123, "110.123m")]
    [DataRow(1e-9, "1n")]
    public void ToString_Double_UsesEngineeringNotation(double input, string expected)
    {
        EngineeringNumber number = input;
        Assert.AreEqual(expected, number.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToString_OutOfPrefixRange_UsesExponent()
    {
        EngineeringNumber number = 1e30;
        Assert.AreEqual("1e30", number.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("1", 1d)]
    [DataRow("1k", 1000d)]
    [DataRow("1.001k", 1001d)]
    [DataRow("1.001M", 1001000d)]
    [DataRow("110m", 0.110)]
    [DataRow("110.123m", 0.110123)]
    public void TryParse_EngineeringNotation_ReturnsValue(string input, double expected)
    {
        Assert.IsTrue(EngineeringNumber.TryParse(input, out var value));
        Assert.AreEqual(expected, (double)value!);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null)]
    [DataRow(" ")]
    [DataRow("xk")]
    [DataRow("abc")]
    public void TryParse_Invalid_ReturnsFalse(string? input) =>
        Assert.IsFalse(EngineeringNumber.TryParse(input, out _));
}

[TestClass]
public class ScientificNumberTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(1d, "1")]
    [DataRow(1000d, "1e3")]
    [DataRow(1001d, "1.001e3")]
    [DataRow(1001000d, "1.001e6")]
    [DataRow(0.110, "1.1e-1")]
    [DataRow(0.110123, "1.101e-1")]
    public void ToString_Double_UsesScientificNotation(double input, string expected)
    {
        ScientificNumber number = input;
        Assert.AreEqual(expected, number.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("1", 1d)]
    [DataRow("1e3", 1000d)]
    [DataRow("1.001e6", 1001000d)]
    [DataRow("1.1e-1", 0.110)]
    public void TryParse_ScientificNotation_ReturnsValue(string input, double expected)
    {
        Assert.IsTrue(ScientificNumber.TryParse(input, out var value));
        Assert.AreEqual(expected, (double)value!);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryParse_Invalid_ReturnsFalse()
    {
        Assert.IsFalse(ScientificNumber.TryParse("abc", out var value));
        Assert.IsNull(value);
    }
}

[TestClass]
public class FormattableNumberTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("1e3", 1000d)]
    [DataRow("1.1e-1", 0.110)]
    [DataRow("1k", 1000d)]
    [DataRow("110m", 0.110)]
    [DataRow("1.001M", 1001000d)]
    [DataRow("1b", 1d)]
    [DataRow("1kb", 1024d)]
    [DataRow("112.64mb", 0.110d)]
    public void Parse_AnyNotation_ReturnsValue(string input, double expected) =>
        Assert.AreEqual(expected, FormattableNumber.Parse(input)!.AsDouble());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_Empty_ReturnsNull() =>
        Assert.IsNull(FormattableNumber.Parse("  "));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Parse_Garbage_Throws() =>
        Assert.ThrowsExactly<NotSupportedException>(() => FormattableNumber.Parse("garbage"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_Null_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new FormattableNumber(null!));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_UnsupportedType_Throws() =>
        Assert.ThrowsExactly<NotSupportedException>(() => new FormattableNumber("text"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Conversions_Double_ConvertsToEveryType()
    {
        var number = new FormattableNumber(1234.5d);

        Assert.AreEqual(1234.5d, number.AsDouble());
        Assert.AreEqual(1234.5f, number.AsSingle());
        Assert.AreEqual(1234.5m, number.AsDecimal());
        Assert.AreEqual((short)1234, number.AsInt16());
        Assert.AreEqual(1234, number.AsInt32());
        Assert.AreEqual(1234L, number.AsInt64());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Conversions_Decimal_KeepsPrecision()
    {
        var number = new FormattableNumber(1234.5m);

        Assert.AreEqual(1234.5m, number.AsDecimal());
        Assert.AreEqual(1234, number.AsInt32());
        Assert.AreEqual(1234.5d, number.AsDouble());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_OtherNumeric_CopiesValueAndProvider()
    {
        var source = new EngineeringNumber(1500d);

        var copy = new FormattableNumber(source);

        Assert.AreEqual(1500d, copy.AsDouble());
        Assert.AreEqual("1.5k", copy.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_ForeignNumeric_UsesDecimalValue()
    {
        var copy = new FormattableNumber(new StubNumeric());

        Assert.AreEqual(7m, copy.AsDecimal());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToString_PlainFormatProvider_UsesNumericFormat()
    {
        var number = new FormattableNumber(1234.5678d);

        Assert.AreEqual("1234.57", number.ToString("0.00", CultureInfo.InvariantCulture));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToString_Format_UsesNotationFormatter()
    {
        var number = new EngineeringNumber(1500d);

        Assert.AreEqual("1.50k", number.ToString("0.00"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ImplicitConversion_BetweenNotations_KeepsValue()
    {
        ByteNumber bytes = new EngineeringNumber(2048d);
        ScientificNumber scientific = bytes;
        EngineeringNumber engineering = scientific;

        Assert.AreEqual(2048d, (double)engineering);
        Assert.AreEqual("2kb", bytes.ToString());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ImplicitConversion_FromIntegers_Works()
    {
        EngineeringNumber fromInt = 2000;
        EngineeringNumber fromLong = 2000L;
        EngineeringNumber fromShort = (short)2000;
        EngineeringNumber fromFloat = 2000f;
        EngineeringNumber fromDecimal = 2000m;

        Assert.AreEqual("2k", fromInt.ToString());
        Assert.AreEqual(2000d, new EngineeringNumber(2000).AsDouble());
        Assert.AreEqual(2000L, (long)fromLong);
        Assert.AreEqual((short)2000, (short)fromShort);
        Assert.AreEqual(2000f, (float)fromFloat);
        Assert.AreEqual(2000m, (decimal)fromDecimal);
        Assert.AreEqual(2000, (int)fromInt);
    }

    private sealed class StubNumeric : INumeric
    {
        public double AsDouble() => 7;
        public float AsSingle() => 7;
        public decimal AsDecimal() => 7;
        public short AsInt16() => 7;
        public int AsInt32() => 7;
        public long AsInt64() => 7;
        public string ToString(string? format, IFormatProvider? formatProvider) => "7";
    }
}

[TestClass]
public class NumberConversionTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ImplicitConversion_ByteNumber_RoundTripsEveryType()
    {
        ByteNumber fromDouble = 2d;
        ByteNumber fromFloat = 2f;
        ByteNumber fromDecimal = 2m;
        ByteNumber fromShort = (short)2;
        ByteNumber fromInt = 2;
        ByteNumber fromLong = 2L;
        ByteNumber fromScientific = new ScientificNumber(2);

        Assert.AreEqual(2d, (double)fromDouble);
        Assert.AreEqual(2f, (float)fromFloat);
        Assert.AreEqual(2m, (decimal)fromDecimal);
        Assert.AreEqual((short)2, (short)fromShort);
        Assert.AreEqual(2, (int)fromInt);
        Assert.AreEqual(2L, (long)fromLong);
        Assert.AreEqual(2d, (double)fromScientific);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ImplicitConversion_ScientificNumber_RoundTripsEveryType()
    {
        ScientificNumber fromDouble = 2d;
        ScientificNumber fromFloat = 2f;
        ScientificNumber fromDecimal = 2m;
        ScientificNumber fromShort = (short)2;
        ScientificNumber fromInt = 2;
        ScientificNumber fromLong = 2L;
        ScientificNumber fromByte = new ByteNumber(2);

        Assert.AreEqual(2d, (double)fromDouble);
        Assert.AreEqual(2f, (float)fromFloat);
        Assert.AreEqual(2m, (decimal)fromDecimal);
        Assert.AreEqual((short)2, (short)fromShort);
        Assert.AreEqual(2, (int)fromInt);
        Assert.AreEqual(2L, (long)fromLong);
        Assert.AreEqual(2d, (double)fromByte);
    }
}
