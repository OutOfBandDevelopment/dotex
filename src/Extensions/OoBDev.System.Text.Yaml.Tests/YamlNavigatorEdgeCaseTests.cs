using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.Text.Yaml.Tests;

[TestCategory(TestCategories.Unit)]
[TestClass]
public class YamlNavigatorEdgeCaseTests
{
    private static MemoryStream Yaml(string text) => new(Encoding.UTF8.GetBytes(text));

    [TestMethod]
    public void ToNavigable_KeyThatIsNotAnXmlName_DoesNotThrow()
    {
        // Stage
        var nav = new YamlNavigator();

        // Test
        var xpath = nav.ToNavigable(Yaml("my key: 1\n2nd: 2\nplain: 3\n"))!.CreateNavigator()!;

        // Assert
        Assert.IsNotNull(xpath.SelectSingleNode("//plain"));
    }

    [TestMethod]
    public void ToNavigable_MultipleDocuments_ReadsTheFirst()
    {
        // Stage
        var nav = new YamlNavigator();

        // Test
        var xpath = nav.ToNavigable(Yaml("a: 1\n---\nb: 2\n"))!.CreateNavigator()!;

        // Assert
        Assert.IsNotNull(xpath.SelectSingleNode("//a"));
    }
}
