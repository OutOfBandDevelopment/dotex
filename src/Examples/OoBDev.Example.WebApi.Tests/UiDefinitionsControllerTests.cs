using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.UiDefinitions;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class UiDefinitionsControllerTests
{
    private static UiDefinitionsController Controller() => new(new UiDefinitionCatalog(), new UiDefinitionEvaluator());

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_ReturnsSamples()
    {
        var result = (OkObjectResult)Controller().List().Result!;

        Assert.AreEqual(2, ((IReadOnlyList<UiDefinitionInfoModel>)result.Value!).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Get_KnownKey_ReturnsDefinition()
    {
        var result = (OkObjectResult)Controller().Get("bench-supply").Result!;

        Assert.AreEqual("Bench power supply", ((UiDefinition)result.Value!).Name);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Get_UnknownKey_NotFound()
    {
        Assert.IsInstanceOfType<NotFoundResult>(Controller().Get("nope").Result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_KnownKey_ReturnsStates()
    {
        var request = new UiEvaluateRequestModel { Values = new() { ["output"] = "true" } };

        var result = (OkObjectResult)Controller().Evaluate("bench-supply", request).Result!;

        var states = (IReadOnlyList<UiControlStateModel>)result.Value!;
        Assert.IsTrue(states.Single(s => s.Id == "voltage").Enabled);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Evaluate_UnknownKey_NotFound()
    {
        Assert.IsInstanceOfType<NotFoundResult>(Controller().Evaluate("nope", new UiEvaluateRequestModel()).Result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Get_Definition_RoundTripsThroughTheFrameworkSerializer()
    {
        var definition = (UiDefinition)((OkObjectResult)Controller().Get("bench-supply").Result!).Value!;

        var back = UiDefinitionSerializer.FromJson(UiDefinitionSerializer.ToJson(definition));

        Assert.AreEqual(definition.Sections.Sum(s => s.Controls.Count), back.Sections.Sum(s => s.Controls.Count));
    }
}
