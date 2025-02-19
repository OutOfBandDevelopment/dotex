using OoBDev.System.Tests.TestTargets;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace OoBDev.System.Tests.Text.Json;

[TestClass]
public class BsonIdConverterTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SerializeTest()
    {
        var expected = "Hello World";

        var result = JsonSerializer.Serialize(new TestIdProperty
        {
            ProjectId = expected,
        });

        TestContext.AddResult(result, fileName: "result.json");

        var document = JsonDocument.Parse(result);
        var selected = document.RootElement.GetProperty("_id").GetProperty("$oid").GetString();
        TestContext.WriteLine(selected);

        Assert.AreEqual(expected, selected);
    }

    [DataTestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow(@"{""_id"":{""$oid"":""Hello World""}}", "Hello World")]
    [DataRow(@"{""_id"":""Hello World""}", "Hello World")]
    public void DeserializeTest(string input, string expected)
    {
        TestContext.AddResult(input, fileName: "input.json");
        var result = JsonSerializer.Deserialize<TestIdProperty>(input);
        TestContext.AddResult(result, fileName: "result.json");
        Assert.AreEqual(expected, result?.ProjectId);
    }
}
