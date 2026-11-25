using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class GenerationControllerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Generate_SameSeedAndType_GivesTheSameGraph()
    {
        // Test
        var first = new GenerationController().Generate(new GenerationRequestModel("customer", 7));
        var second = new GenerationController().Generate(new GenerationRequestModel("Customer", 7));
        TestContext.WriteLine(first.Value?.Value.GetRawText());

        // Assert
        Assert.IsNotNull(first.Value);
        Assert.IsNotNull(second.Value);
        Assert.AreEqual(first.Value.Value.GetRawText(), second.Value.Value.GetRawText());
        Assert.IsTrue(first.Value.Value.GetProperty("Orders").GetArrayLength() >= 1);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Generate_DifferentSeeds_GiveDifferentGraphs()
    {
        // Test
        var a = new GenerationController().Generate(new GenerationRequestModel("person", 1));
        var b = new GenerationController().Generate(new GenerationRequestModel("person", 2));

        // Assert
        Assert.AreNotEqual(a.Value!.Value.GetRawText(), b.Value!.Value.GetRawText());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Generate_Person_FollowsTheAttributes()
    {
        // Test
        var person = new GenerationController().Generate(new GenerationRequestModel("person", 3)).Value!.Value;

        // Assert
        Assert.Contains("@", person.GetProperty("Email").GetString());
        Assert.IsTrue(person.GetProperty("Age").GetInt32() is >= 18 and <= 90);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Generate_UnknownType_ReturnsProblem()
    {
        // Test
        var result = new GenerationController().Generate(new GenerationRequestModel("spaceship", 1));

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(400, objectResult.StatusCode);
    }
}
