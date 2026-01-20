using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Utilities;
using OoBDev.TestUtilities;
using System.Reflection;

namespace OoBDev.System.Tests.Utilities;

[TestClass]
public class StringFormatterTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Format_SimpleParameter_ReplacesCorrectly()
    {
        // Arrange
        var formatter = new StringFormatter();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.SimpleMethod))!;
        var args = new object[] { "hello" };

        // Act
        var result = formatter.Format("BaseKey::{arg}", method, args);

        // Assert
        Assert.AreEqual("BaseKey::hello", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Format_SinglePropertyAccess_ReplacesCorrectly()
    {
        // Arrange
        var formatter = new StringFormatter();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.ModelMethod))!;
        var model = new TestModel { Name = "Matt" };
        var args = new object[] { "hello", model };

        // Act
        var result = formatter.Format("BaseKey::{arg}::{model.Name}", method, args);

        // Assert
        Assert.AreEqual("BaseKey::hello::Matt", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Format_PropertyChain_ReplacesCorrectly()
    {
        // Arrange
        var formatter = new StringFormatter();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.NestedModelMethod))!;
        var model = new TestNestedModel
        {
            User = new TestModel { Name = "Matt" }
        };
        var args = new object[] { "hello", model };

        // Act
        var result = formatter.Format("BaseKey::{arg}::{model.User.Name}", method, args);

        // Assert
        Assert.AreEqual("BaseKey::hello::Matt", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Format_DeepPropertyChain_ReplacesCorrectly()
    {
        // Arrange
        var formatter = new StringFormatter();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.DeepModelMethod))!;
        var model = new TestDeepModel
        {
            Company = new TestCompany
            {
                Address = new TestAddress
                {
                    City = "Seattle"
                }
            }
        };
        var args = new object[] { model };

        // Act
        var result = formatter.Format("companies/{model.Company.Address.City}", method, args);

        // Assert
        Assert.AreEqual("companies/Seattle", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Format_MultiplePropertyChains_ReplacesAllCorrectly()
    {
        // Arrange
        var formatter = new StringFormatter();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.ComplexMethod))!;
        var user = new TestModel { Name = "Matt" };
        var company = new TestCompany { Address = new TestAddress { City = "Seattle" } };
        var args = new object[] { "prefix", user, company };

        // Act
        var result = formatter.Format("{prefix}::{user.Name}::{company.Address.City}", method, args);

        // Assert
        Assert.AreEqual("prefix::Matt::Seattle", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Format_NullPropertyInChain_ReturnsEmptyString()
    {
        // Arrange
        var formatter = new StringFormatter();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.DeepModelMethod))!;
        var model = new TestDeepModel
        {
            Company = null  // Null in the chain
        };
        var args = new object[] { model };

        // Act
        var result = formatter.Format("companies/{model.Company.Address.City}", method, args);

        // Assert
        Assert.AreEqual("companies/", result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Format_NullParameter_ReturnsEmptyString()
    {
        // Arrange
        var formatter = new StringFormatter();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.ModelMethod))!;
        var args = new object[] { "hello", null! };

        // Act
        var result = formatter.Format("BaseKey::{arg}::{model.Name}", method, args);

        // Assert
        Assert.AreEqual("BaseKey::hello::", result);
    }

    public class TestClass
    {
        public void SimpleMethod(string arg) { }
        public void ModelMethod(string arg, TestModel model) { }
        public void NestedModelMethod(string arg, TestNestedModel model) { }
        public void DeepModelMethod(TestDeepModel model) { }
        public void ComplexMethod(string prefix, TestModel user, TestCompany company) { }
    }

    public class TestModel
    {
        public string Name { get; set; } = string.Empty;
    }

    public class TestNestedModel
    {
        public TestModel? User { get; set; }
    }

    public class TestAddress
    {
        public string City { get; set; } = string.Empty;
    }

    public class TestCompany
    {
        public TestAddress? Address { get; set; }
    }

    public class TestDeepModel
    {
        public TestCompany? Company { get; set; }
    }
}
