using OoBDev.Extensions.IO;
using OoBDev.Extensions.Reflection;
using OoBDev.System.Tests.Reflection.TestTargets;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.Reflection;

[TestClass]
public class ResourceExtensionsTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task GetResourceAsStringAsyncTest()
    {
        var result = await this.GetResourceAsStringAsync("SampleResource.txt");
        Assert.AreEqual("Hello World!", result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetResourceStreamTest()
    {
        using var result = this.GetResourceStream("SampleResource.json");
        Assert.IsNotNull(result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetResourceStreamTest_ByType()
    {
        using var result = GetType().GetResourceStream("SampleResource.json");
        Assert.IsNotNull(result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetResourceStreamTest_NotFound()
    {
        using var result = this.GetResourceStream("SampleResource.fake");
        Assert.IsNull(result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task GetResourceFromJsonAsyncTest()
    {
        var result = await this.GetResourceStream("SampleResource.json").AsJsonAsync<TestModel>();
        Assert.AreEqual("Hello World!", result?.Property);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task GetResourceFromJsonAsyncTest_NotFound()
    {
        var result = await this.GetResourceStream("SampleResource.fake").AsJsonAsync<TestModel>();
        Assert.IsNull(result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task GetResourceFromXmlAsyncTest()
    {
        var result = await this.GetResourceStream("SampleResource.xml").AsXmlAsync<TestModel>();
        Assert.AreEqual("Hello World!", result?.Property);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetResourceFromJsonTest()
    {
        var result = this.GetResourceStream("SampleResource.json").AsJson<TestModel>();
        Assert.AreEqual("Hello World!", result?.Property);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetResourceFromJsonTest_NotFound()
    {
        var result = this.GetResourceStream("SampleResource.fake").AsJson<TestModel>();
        Assert.IsNull(result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetResourceFromXmlTest()
    {
        var result = this.GetResourceStream("SampleResource.xml").AsXml<TestModel>();
        Assert.AreEqual("Hello World!", result?.Property);
    }
}
