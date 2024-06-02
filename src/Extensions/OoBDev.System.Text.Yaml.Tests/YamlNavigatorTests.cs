using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.IO;

namespace OoBDev.System.Text.Yaml.Tests;

[TestClass]
public class YamlNavigatorTests
{
    public required TestContext TestContext { get; set; }

    [DataTestMethod, TestCategory(TestCategories.DevLocal)]
    [DataRow("Example.yml")]
    public void ToNavigableTest(string resourceName)
    {
        var nav = new YamlNavigator();
        var stream = this.GetResourceStream(resourceName);
        var xpath = nav.ToNavigable(stream).CreateNavigator();
        TestContext.AddResult(xpath, Path.ChangeExtension(resourceName, ".xml"));
    }
}
