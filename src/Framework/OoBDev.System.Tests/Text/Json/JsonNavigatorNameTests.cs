using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Text.Json;
using OoBDev.TestUtilities;
using System.Text.Json;

namespace OoBDev.System.Tests.Text.Json;

[TestCategory(TestCategories.Unit)]
[TestClass]
public class JsonNavigatorNameTests
{
    [TestMethod]
    public void ToNavigable_PropertyNameThatIsNotAnXmlName_DoesNotThrow()
    {
        // Stage
        using var json = JsonDocument.Parse("""{"my key":1,"2nd":2,"plain":3}""");

        // Test
        var xpath = json.ToNavigable().CreateNavigator()!;

        // Assert
        Assert.IsNotNull(xpath.SelectSingleNode("//plain"));
    }
}
