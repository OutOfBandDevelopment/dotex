using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Net.SecurityManagement;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.Net.SecurityManagement;

[TestClass]
public class LdapSimpleFilterTests
{
    [TestMethod, TestCategory(TestCategories.Unit), TestCategory(TestCategories.BugRegression)]
    public void Equals_DifferentCase_IsEqualWithTheSameHashCode()
    {
        // Stage
        var left = new LdapSimpleFilter("CN", LdapFilterTypes.Equals, "Alice");
        var right = new LdapSimpleFilter("cn", LdapFilterTypes.Equals, "ALICE");

        // Assert
        Assert.AreEqual(left, right);
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Equals_DifferentValue_IsNotEqual()
    {
        // Assert
        Assert.AreNotEqual(
            new LdapSimpleFilter("cn", LdapFilterTypes.Equals, "alice"),
            new LdapSimpleFilter("cn", LdapFilterTypes.Equals, "bob"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Build_SimpleFilter_WritesTheLdapText()
    {
        // Test
        var text = new LdapFilterBuilder().Build(new LdapSimpleFilter("cn", LdapFilterTypes.Equals, "alice"));

        // Assert
        Assert.AreEqual("(cn=alice)", text);
    }
}
