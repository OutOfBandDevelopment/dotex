using OoBDev.System.Text.Xml.Serialization;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.Text.Xml.Serialization;

[TestClass]
public class DefaultXmlSerializerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DeserializeTest_StringType()
    {
        var serializer = new DefaultXmlSerializer();
        var result = serializer.Deserialize(SourceText, typeof(TestTarget));
        Assert.IsInstanceOfType<TestTarget>(result);
        Assert.AreEqual("value1", ((TestTarget)result).Prop1);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DeserializeTest_StringGeneric()
    {
        var serializer = new DefaultXmlSerializer();
        var result = serializer.Deserialize<TestTarget>(SourceText);
        Assert.AreEqual("value1", result?.Prop1);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DeserializeTest_StreamType()
    {
        var serializer = new DefaultXmlSerializer();
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms) { AutoFlush = true };
        writer.Write(SourceText);
        ms.Position = 0;
        var result = serializer.Deserialize(ms, typeof(TestTarget));
        Assert.IsInstanceOfType<TestTarget>(result);
        Assert.AreEqual("value1", ((TestTarget)result).Prop1);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DeserializeTest_StreamGeneric()
    {
        var serializer = new DefaultXmlSerializer();
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms) { AutoFlush = true };
        writer.Write(SourceText);
        ms.Position = 0;
        var result = serializer.Deserialize<TestTarget>(ms);
        Assert.AreEqual("value1", result?.Prop1);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task DeserializeAsyncTest_StreamType()
    {
        var serializer = new DefaultXmlSerializer();
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms) { AutoFlush = true };
        writer.Write(SourceText);
        ms.Position = 0;
        var result = await serializer.DeserializeAsync(ms, typeof(TestTarget));
        Assert.IsInstanceOfType<TestTarget>(result);
        Assert.AreEqual("value1", ((TestTarget)result).Prop1);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task DeserializeAsyncTest_StreamGeneric()
    {
        var serializer = new DefaultXmlSerializer();
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms) { AutoFlush = true };
        writer.Write(SourceText);
        ms.Position = 0;
        var result = await serializer.DeserializeAsync<TestTarget>(ms);
        Assert.AreEqual("value1", result?.Prop1);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SerializeTest_Type()
    {
        var obj = new TestTarget { Prop1 = "value1" };
        var serializer = new DefaultXmlSerializer();
        var result = serializer.Serialize(obj, typeof(TestTarget));
        Assert.IsNotNull(result);
        TestContext.WriteLine(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void SerializeTest_Generic()
    {
        var obj = new TestTarget { Prop1 = "value1" };
        var serializer = new DefaultXmlSerializer();
        var result = serializer.Serialize(obj);
        Assert.IsNotNull(result);
        TestContext.WriteLine(result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SerializeAsyncTest_Type()
    {
        var obj = new TestTarget { Prop1 = "value1" };
        var ms = new MemoryStream();
        var serializer = new DefaultXmlSerializer();
        var result = serializer.SerializeAsync(obj, typeof(TestTarget), ms);
        Assert.IsNotNull(result);

        ms.Position = 0;
        var reader = new StreamReader(ms);
        var read = await reader.ReadToEndAsync();

        TestContext.WriteLine(read);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SerializeAsyncTest_Generic()
    {
        var obj = new TestTarget { Prop1 = "value1" };
        var ms = new MemoryStream();
        var serializer = new DefaultXmlSerializer();
        var result = serializer.SerializeAsync(obj, ms);
        Assert.IsNotNull(result);

        ms.Position = 0;
        var reader = new StreamReader(ms);
        var read = await reader.ReadToEndAsync();

        TestContext.WriteLine(read);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void ContentTypeTest()
    {
        var serializer = new DefaultXmlSerializer();
        Assert.AreEqual("text/xml", serializer.ContentType);
    }

    public const string SourceText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<TestTarget xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <Prop1>value1</Prop1>
</TestTarget>";

    public class TestTarget
    {
        public required string Prop1 { get; set; }
    }
}
