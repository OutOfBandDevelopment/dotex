using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.ComponentModel;
using OoBDev.System.Text;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.System.Tests.ComponentModel;

[TestClass]
public class ObjectConverterTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_InputAlreadyTargetType_ReturnsSameInstanceWithoutSerializing()
    {
        TestContext.WriteLine("Stage: a string input and a strict serializer mock that fails on any call");
        var serializer = new Mock<ISerializer>(MockBehavior.Strict);
        var converter = new ObjectConverter(serializer.Object);

        TestContext.WriteLine("Test: convert a string to string");
        var input = "value";
        var result = converter.Convert(input, typeof(string));

        TestContext.WriteLine($"Assert: same instance returned, result '{result}'");
        Assert.AreSame(input, result);
        serializer.VerifyNoOtherCalls();
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_StringInput_DeserializesTheStringDirectly()
    {
        TestContext.WriteLine("Stage: serializer that deserializes '{\"a\":1}' to a Version marker");
        var expected = new Version(1, 2);
        var serializer = new Mock<ISerializer>(MockBehavior.Strict);
        serializer.Setup(s => s.Deserialize("{\"a\":1}", typeof(Version))).Returns(expected);
        var converter = new ObjectConverter(serializer.Object);

        TestContext.WriteLine("Test: convert a json string to Version");
        var result = converter.Convert("{\"a\":1}", typeof(Version));

        TestContext.WriteLine($"Assert: deserialized result {result}");
        Assert.AreSame(expected, result);
        serializer.Verify(s => s.Deserialize("{\"a\":1}", typeof(Version)), Times.Once);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_OtherObject_SerializesThenDeserializes()
    {
        TestContext.WriteLine("Stage: serializer that turns an int into '5' and '5' into a Version");
        var expected = new Version(5, 0);
        var serializer = new Mock<ISerializer>(MockBehavior.Strict);
        serializer.Setup(s => s.Serialize<object>(5)).Returns("5");
        serializer.Setup(s => s.Deserialize("5", typeof(Version))).Returns(expected);
        var converter = new ObjectConverter(serializer.Object);

        TestContext.WriteLine("Test: convert 5 to Version");
        var result = converter.Convert(5, typeof(Version));

        TestContext.WriteLine($"Assert: result {result}");
        Assert.AreSame(expected, result);
        serializer.VerifyAll();
    }
}
