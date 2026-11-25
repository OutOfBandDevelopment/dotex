using OoBDev.System.Tests.TestTargets;
using OoBDev.System.Text.Json;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace OoBDev.System.Tests.Text.Json;

[TestClass]
public class BsonSerializerTests
{
    public required TestContext TestContext { get; set; }

    private static JsonSerializerOptions GetOptions() => new()
    {
        TypeInfoResolver = new BsonTypeInfoResolver(),
        WriteIndented = true,
    };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Test()
    {
        var model = new TargetModel();
        var json = JsonSerializer.Serialize(model, model.GetType(), GetOptions());

        TestContext.WriteLine(json);
    }

    private sealed class CustomIdAttribute : global::System.Attribute
    {
    }

    private sealed class Plain
    {
        [CustomId]
        public string Code { get; set; } = "A1";
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetTypeInfo_CustomIdAttributeName_UsesTheIdConverter()
    {
        TestContext.WriteLine("Stage: a resolver told that CustomIdAttribute marks an identifier");
        var resolver = new BsonTypeInfoResolver(["CustomIdAttribute"]);
        var options = new JsonSerializerOptions { TypeInfoResolver = resolver };

        var info = resolver.GetTypeInfo(typeof(Plain), options);

        var converter = info.Properties[0].CustomConverter;
        TestContext.WriteLine($"converter {converter?.GetType().Name}");
        Assert.IsInstanceOfType<BsonIdConverter>(converter);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetTypeInfo_DefaultResolver_IgnoresUnknownAttribute()
    {
        var resolver = new BsonTypeInfoResolver();
        var options = new JsonSerializerOptions { TypeInfoResolver = resolver };

        var info = resolver.GetTypeInfo(typeof(Plain), options);

        Assert.IsNull(info.Properties[0].CustomConverter);
    }
}
