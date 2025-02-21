using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.TestUtilities.Tests;

[TestClass]
public class TextContextExtensionsTests
{
    public TestContext TestContext { get; set; }

    public class TestData
    {
        public string Property1 { get; set; } = "Hello World!";
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Object()
    {
        // Stage
        var value = new TestData();

        // Mock

        // Test
        var context = TestContext.AddResult(value);

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.json").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_String()
    {
        // Stage
        var value = "Hello World!";

        // Mock

        // Test
        var context = TestContext.AddResult(value);

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.json").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Lines()
    {
        // Stage
        var value = new[] { "Hello World!", "Goodbye" };

        // Mock

        // Test
        var context = TestContext.AddResult(value);

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.json").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Object_WithFileNameNoExtension()
    {
        // Stage
        var value = new TestData();

        // Mock

        // Test
        var context = TestContext.AddResult(value, fileName: "TestFileName");

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.json").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Object_WithFileNameAndExtension()
    {
        // Stage
        var value = new TestData();

        // Mock

        // Test
        var context = TestContext.AddResult(value, fileName: "TestFileName.txt");

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.txt").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Object_WithFileNameAndChangeExtension()
    {
        // Stage
        var value = new TestData();

        // Mock

        // Test
        var context = TestContext.AddResult(value, fileName: "TestFileName.html");

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.html").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Object_WithFileNameRemoveExtension()
    {
        // Stage
        var value = new TestData();

        // Mock

        // Test
        var context = TestContext.AddResult(value, fileName: "TestFileName.");

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Stream()
    {
        // Stage
        Stream value = new MemoryStream(Encoding.UTF8.GetBytes("Test File"));

        // Mock

        // Test
        var context = TestContext.AddResult(value);

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.bin").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_Json()
    {
        // Stage
        var value = new JObject(new JProperty("key", "value"));

        // Mock

        // Test
        var context = TestContext.AddResult(value);

        // Assert
        Assert.AreEqual(TestContext, context);

        Assert.IsTrue(Directory.EnumerateFiles(context.TestRunResultsDirectory, "*.json").Any());

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultTest_ValueOutFile()
    {
        // Stage
        var value = new TestData();

        // Mock

        // Test
        var context = TestContext.AddResult(value, out var outFile);

        TestContext?.WriteLine(outFile);

        // Assert
        Assert.AreEqual(TestContext, context);
        Assert.IsTrue(File.Exists(outFile));

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultFileTest_FileContent()
    {
        // Stage
        var fileName = "test-file.txt";
        var content = Encoding.UTF8.GetBytes("Hello World!");

        // Mock

        // Test
        var context = TestContext.AddResultFile(fileName, content);

        // Assert
        Assert.AreEqual(TestContext, context);
        Assert.IsTrue(File.Exists(Path.Combine(context.TestRunResultsDirectory, fileName)));

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void AddResultFileTest_FileContentOutFile()
    {
        // Stage
        var fileName = "test-file.txt";
        var content = Encoding.UTF8.GetBytes("Hello World!");

        // Mock

        // Test

        var context = TestContext.AddResultFile(fileName, content, out var outFile);
        TestContext?.WriteLine(outFile);

        // Assert
        Assert.AreEqual(TestContext, context);
        Assert.IsTrue(File.Exists(outFile));

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetTestDataAsyncTest()
    {
        // Stage

        // Mock

        // Test
        var result = await TestContext.GetTestDataAsync<TestData>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Hello World!", result.Property1);

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetTestDataAsyncTest_Targeted()
    {
        // Stage
        var target = "TextContextExtensionsTests_GetTestDataAsyncTest";

        // Mock

        // Test

        var result = await TestContext.GetTestDataAsync<TestData>(target);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Hello World!", result.Property1);

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetTestDataAsyncTest_String()
    {
        // Stage

        // Mock

        // Test

        var result = await TestContext.GetTestDataAsync<string>();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Hello World!", result);

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetTestDataAsyncTest_Stream()
    {
        // Stage

        // Mock

        // Test

        var result = await TestContext.GetTestDataAsync<Stream>();
        var reader = new StreamReader(result);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Hello World!", reader.ReadToEnd());

        // Verify
    }
}
