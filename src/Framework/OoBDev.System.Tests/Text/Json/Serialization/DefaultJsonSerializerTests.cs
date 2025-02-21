using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace OoBDev.System.Tests.Text.Json.Serialization;

[TestClass]
public class DefaultJsonSerializerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SerializeTest_Anonymous()
    {
        var serializer = new DefaultJsonSerializer();

        var input = new { Hello = "world" };
        var (serialized, result) = InOut(serializer, input);

        TestContext.AddResult(serialized);
        TestContext.AddResult(result);

        Assert.AreEqual(input, result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SerializeTest_AnonymousWithStringObjectDictionary()
    {
        var serializer = new DefaultJsonSerializer();

        var input = new
        {
            Hello = "world",
            Dict = new Dictionary<string, object>
            {
                {"one", 1L },
                {"two", 2L},
                { "three", "3"},
            },
        };

        var (serialized, result) = InOut(serializer, input);

        TestContext.AddResult(serialized);
        TestContext.AddResult(result);

        Assert.AreEqual(input.Hello, result?.Hello);

        Assert.AreEqual(input.Dict["one"], result?.Dict["one"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SerializeTest_IConfigurationSection()
    {
        var serializer = new DefaultJsonSerializer();

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"Configuration:Property", "test-value" },
        });
        var config = configBuilder.Build();
        var input = config.GetSection("Configuration");

        var (serialized, result) = InOut(serializer, input);

        TestContext.AddResult(serialized);
        TestContext.AddResult(result);

        Assert.AreEqual(input["Property"], result?["Property"]);
    }

    private static (string serialized, T? result) InOut<T>(DefaultJsonSerializer serializer, T input)
    {
        var serialized = serializer.Serialize(input);
        var result = serializer.Deserialize<T>(serialized);
        return (serialized, result);
    }
}
