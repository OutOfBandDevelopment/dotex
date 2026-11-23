using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class DistanceControllerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Measure_LondonToParis_ReturnsAbout344KmSouthEast()
    {
        // Test
        var result = new DistanceController().Measure(new DistanceRequestModel(51.5074, -0.1278, 48.8566, 2.3522));
        TestContext.WriteLine($"km={result.Value?.Kilometers} bearing={result.Value?.BearingDegrees}");

        // Assert
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(343.6, result.Value.Kilometers, 1.5);
        Assert.AreEqual(result.Value.Meters / 1609.344, result.Value.Miles, 1e-9);
        Assert.AreEqual(148.1, result.Value.BearingDegrees, 0.5);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Measure_LatitudeOutOfRange_ReturnsProblem()
    {
        // Test
        var result = new DistanceController().Measure(new DistanceRequestModel(95, 0, 0, 0));

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(400, objectResult.StatusCode);
    }
}
