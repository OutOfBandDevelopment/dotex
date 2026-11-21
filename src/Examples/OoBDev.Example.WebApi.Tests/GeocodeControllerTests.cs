using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.SpatialServices;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class GeocodeControllerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Geocode_Match_ReturnsCoordinatesAndNormalizedAddress()
    {
        // Mock
        var locations = new Mock<ILocationServices>();
        locations.Setup(l => l.GeocodeAsync(It.IsAny<AddressQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodeResult(new GlobalPosition(38.8977, -77.0365), ResultQuality.Interpolated, "1600 PENNSYLVANIA AVE NW, WASHINGTON, DC, 20500"));

        // Test
        var result = await new GeocodeController(locations.Object).Geocode(new GeocodeRequestModel(" 1600 Pennsylvania Ave NW ", "Washington", "DC", "20500"), CancellationToken.None);
        TestContext.WriteLine($"Matched={result.Value?.Matched} Lat={result.Value?.Latitude} Lon={result.Value?.Longitude}");

        // Assert
        Assert.IsNotNull(result.Value);
        Assert.IsTrue(result.Value.Matched);
        Assert.AreEqual(38.8977, result.Value.Latitude);
        Assert.AreEqual(-77.0365, result.Value.Longitude);
        Assert.AreEqual("Interpolated", result.Value.Quality);
        locations.Verify(l => l.GeocodeAsync(It.Is<AddressQuery>(a => a.Line1 == "1600 Pennsylvania Ave NW" && a.City == "Washington"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Geocode_NoMatch_ReturnsMatchedFalse()
    {
        // Mock
        var locations = new Mock<ILocationServices>();
        locations.Setup(l => l.GeocodeAsync(It.IsAny<AddressQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(GeocodeResult.NoMatch);

        // Test
        var result = await new GeocodeController(locations.Object).Geocode(new GeocodeRequestModel("nowhere"), CancellationToken.None);

        // Assert
        Assert.IsNotNull(result.Value);
        Assert.IsFalse(result.Value.Matched);
        Assert.IsNull(result.Value.Latitude);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Geocode_BlankLine_IsRefusedWithoutCallingTheService()
    {
        // Mock
        var locations = new Mock<ILocationServices>(MockBehavior.Strict);

        // Test
        var result = await new GeocodeController(locations.Object).Geocode(new GeocodeRequestModel(" "), CancellationToken.None);

        // Assert
        Assert.IsInstanceOfType<ObjectResult>(result.Result);
        Assert.AreEqual(400, ((ObjectResult)result.Result).StatusCode);
    }
}
