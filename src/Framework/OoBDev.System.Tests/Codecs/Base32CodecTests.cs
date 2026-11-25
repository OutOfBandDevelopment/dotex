using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Codecs;
using OoBDev.TestUtilities;
using System;
using System.Text;

namespace OoBDev.System.Tests.Codecs;

[TestClass]
public class Base32CodecTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void DecodeTest()
    {
        var tests = new[]{
            new {encoded="", decoded = "",},
            new {encoded="MY======", decoded ="f",},
            new {encoded="MZXQ====", decoded = "fo",},
            new {encoded="MZXW6===", decoded = "foo",},
            new {encoded="MZXW6YQ=", decoded = "foob",},
            new {encoded="MZXW6YTB", decoded = "fooba",},
            new {encoded="MZXW6YTBOI======", decoded = "foobar",},
        };

        var encoding = new Base32Codec();
        foreach (var test in tests)
        {
            var bytes = encoding.Decode(test.encoded);
            var decoded = Encoding.ASCII.GetString(bytes);
            Assert.AreEqual(test.decoded, decoded);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EncodeTest()
    {
        var tests = new[]{
            new {encoded="", decoded = "",},
            new {encoded="MY======", decoded ="f",},
            new {encoded="MZXQ====", decoded = "fo",},
            new {encoded="MZXW6===", decoded = "foo",},
            new {encoded="MZXW6YQ=", decoded = "foob",},
            new {encoded="MZXW6YTB", decoded = "fooba",},
            new {encoded="MZXW6YTBOI======", decoded = "foobar",},
        };

        var encoding = new Base32Codec();
        foreach (var test in tests)
        {
            var buffer = Encoding.ASCII.GetBytes(test.decoded);
            var encoded = encoding.Encode(buffer);
            Assert.AreEqual(test.encoded, encoded);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EncodeDecode_SpanInputs_MatchTheArrayForms()
    {
        var encoding = new Base32Codec();
        ReadOnlySpan<byte> data = stackalloc byte[] { 1, 2, 3, 4, 5, 6, 7 };

        var fromSpan = encoding.Encode(data);
        var fromArray = encoding.Encode(data.ToArray());
        var decoded = encoding.Decode(fromSpan.AsSpan());

        TestContext.WriteLine(fromSpan);
        Assert.AreEqual(fromArray, fromSpan);
        CollectionAssert.AreEqual(data.ToArray(), decoded);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Decode_UnpaddedInput_DecodesLikePaddedInput()
    {
        var encoding = new Base32Codec();

        CollectionAssert.AreEqual(encoding.Decode("MZXW6YQ="), encoding.Decode("MZXW6YQ"));
    }
}
