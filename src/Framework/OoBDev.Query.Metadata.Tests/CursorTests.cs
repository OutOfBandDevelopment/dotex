using System.Collections.Generic;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Query.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public sealed class CursorTests
{
    private static CursorState State(string kind = "photos", string sort = "-taken,id") =>
        new(kind, sort, [JsonSerializer.SerializeToElement("2024-01-01"), JsonSerializer.SerializeToElement(7)]);

    [TestMethod]
    public void Decode_RoundTrip_ReturnsState()
    {
        var codec = Support.Codec();
        var token = codec.Encode(State());

        var back = codec.Decode(token, "photos", "-taken,id");

        Assert.AreEqual(2, back.Values.Length);
    }

    [TestMethod]
    public void Decode_Tampered_Is400()
    {
        var codec = Support.Codec();
        var token = codec.Encode(State());
        var tampered = "A" + token[1..];

        Assert.ThrowsExactly<QueryException>(() => codec.Decode(tampered, "photos", "-taken,id"));
    }

    [TestMethod]
    public void Decode_OtherKey_Is400()
    {
        var token = Support.Codec().Encode(State());

        Assert.ThrowsExactly<QueryException>(() => Support.Codec("another-key-another-key-another-key-123").Decode(token, "photos", "-taken,id"));
    }

    [TestMethod]
    public void Decode_OtherKind_Is400()
    {
        var codec = Support.Codec();

        var ex = Assert.ThrowsExactly<QueryException>(() => codec.Decode(codec.Encode(State()), "admin", "-taken,id"));

        StringAssert.Contains(ex.Message, "different kind or sort");
    }

    [TestMethod]
    public void Decode_OtherSort_Is400()
    {
        var codec = Support.Codec();

        Assert.ThrowsExactly<QueryException>(() => codec.Decode(codec.Encode(State()), "photos", "name,id"));
    }

    [TestMethod]
    public void Decode_Garbage_Is400()
    {
        var codec = Support.Codec();

        foreach (var s in new[] { "abc", "a.b", "!!.!!" })
            Assert.ThrowsExactly<QueryException>(() => codec.Decode(s, "photos", "x"), s);
    }

    [TestMethod]
    public void Compile_WithValidCursor_AddsKeysetCondition()
    {
        var first = Support.Compile(new QueryRequest { From = "photos" });
        var token = Support.Codec().Encode(new CursorState("photos", first.SortSpec, [JsonSerializer.SerializeToElement("2024-01-01T00:00:00Z"), JsonSerializer.SerializeToElement(5)]));

        var q = Support.Compile(new QueryRequest { From = "photos", After = token });

        Assert.AreNotEqual(first.Sql, q.Sql);
        Assert.IsTrue(q.Parameters.Count > first.Parameters.Count);
    }

    [TestMethod]
    public void Compile_CursorFromOtherSort_Is400()
    {
        var first = Support.Compile(new QueryRequest { From = "photos" });
        var token = Support.Codec().Encode(new CursorState("photos", first.SortSpec, [JsonSerializer.SerializeToElement("x"), JsonSerializer.SerializeToElement(5)]));

        Assert.ThrowsExactly<QueryException>(() => Support.Compile(new QueryRequest { From = "photos", Sort = ["name"], After = token }));
    }

    [TestMethod]
    public void Options_ShortKey_FailsValidation()
    {
        var options = new QueryMetadataOptions { CursorKey = "short" };
        var results = new List<global::System.ComponentModel.DataAnnotations.ValidationResult>();

        Assert.IsFalse(global::System.ComponentModel.DataAnnotations.Validator.TryValidateObject(options, new(options), results, true));
    }
}
