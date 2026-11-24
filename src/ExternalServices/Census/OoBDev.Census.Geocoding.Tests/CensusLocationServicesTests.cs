using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.SpatialServices;

namespace OoBDev.Census.Geocoding.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class CensusLocationServicesTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class Fake(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public Uri? LastUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            return Task.FromResult(respond(request));
        }
    }

    private static HttpResponseMessage Json(string json, HttpStatusCode code = HttpStatusCode.OK)
        => new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static (CensusLocationServices Service, Fake Handler) Create(Func<HttpRequestMessage, HttpResponseMessage> respond)
    {
        var handler = new Fake(respond);
        return (new CensusLocationServices(new HttpClient(handler)), handler);
    }

    private static readonly AddressQuery Address = new("4600 Silver Hill Rd", City: "Washington", Region: "DC", PostalCode: "20233");

    [TestMethod]
    public async Task Geocode_Match_MapsPositionQualityAndAddress()
    {
        var (service, handler) = Create(_ => Json("""{"result":{"addressMatches":[{"matchedAddress":"4600 SILVER HILL RD, WASHINGTON, DC, 20233","coordinates":{"x":-76.92748,"y":38.84601},"tigerLine":{"side":"L"}}]}}"""));

        var result = await service.GeocodeAsync(Address, CancellationToken.None);
        TestContext.WriteLine($"{handler.LastUri} -> {result.Position?.Latitude},{result.Position?.Longitude} {result.Quality}");

        Assert.AreEqual(38.84601, result.Position!.Latitude, 1e-9);
        Assert.AreEqual(-76.92748, result.Position.Longitude, 1e-9);
        Assert.AreEqual(ResultQuality.Interpolated, result.Quality);
        Assert.AreEqual("4600 SILVER HILL RD, WASHINGTON, DC, 20233", result.NormalizedAddress);
        StringAssert.Contains(handler.LastUri!.Query, "address=4600%20Silver%20Hill%20Rd%2C%20Washington");
    }

    [TestMethod]
    public async Task Geocode_NoMatchOrServerErrorOrBadJson_QualityNoneWithoutException()
    {
        foreach (var respond in new Func<HttpRequestMessage, HttpResponseMessage>[]
        {
            _ => Json("""{"result":{"addressMatches":[]}}"""),
            _ => Json("{}", HttpStatusCode.InternalServerError),
            _ => Json("not json"),
            _ => throw new HttpRequestException("down"),
        })
        {
            var (service, _) = Create(respond);

            var result = await service.GeocodeAsync(Address, CancellationToken.None);

            Assert.AreEqual(ResultQuality.None, result.Quality);
            Assert.IsNull(result.Position);
        }
    }

    [TestMethod]
    public async Task Reverse_AlwaysNoMatch()
    {
        var (service, _) = Create(_ => Json("{}"));

        var result = await service.ReverseGeocodeAsync(new GlobalPosition(1, 2), CancellationToken.None);

        Assert.AreEqual(ResultQuality.None, result.Quality);
    }

    [TestMethod]
    public void GlobalPosition_OutOfRange_Rejected()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new GlobalPosition(91, 0));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new GlobalPosition(0, -181));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new GlobalPosition(double.NaN, 0));
    }

    [TestMethod]
    public void ToOneLine_SkipsEmptyParts()
    {
        Assert.AreEqual("1 Main St, Town, 12345", new AddressQuery("1 Main St", null, "Town", null, "12345").ToOneLine());
    }
}
