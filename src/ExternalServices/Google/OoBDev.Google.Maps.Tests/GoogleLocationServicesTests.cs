using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.SpatialServices;

namespace OoBDev.Google.Maps.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class GoogleLocationServicesTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class Fake(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        public Uri? LastUri { get; private set; }

        public int Calls { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastUri = request.RequestUri;
            Calls++;
            return Task.FromResult(respond(request));
        }
    }

    private static HttpResponseMessage Json(string json, HttpStatusCode code = HttpStatusCode.OK)
        => new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };

    private static (GoogleLocationServices Service, Fake Handler) Create(Func<HttpRequestMessage, HttpResponseMessage> respond, string? key = "k1")
    {
        var handler = new Fake(respond);
        var options = Options.Create(new GoogleMapsOptions { ApiKey = key });
        return (new GoogleLocationServices(new HttpClient(handler), options), handler);
    }

    private const string Rooftop = """{"status":"OK","results":[{"formatted_address":"1600 Amphitheatre Pkwy, Mountain View, CA","geometry":{"location":{"lat":37.4224,"lng":-122.0842},"location_type":"ROOFTOP"}}]}""";

    private static readonly AddressQuery Address = new("1600 Amphitheatre Pkwy", City: "Mountain View", Region: "CA");

    [TestMethod]
    public async Task Geocode_Match_MapsPositionQualityAndAddress()
    {
        var (service, handler) = Create(_ => Json(Rooftop));

        var result = await service.GeocodeAsync(Address, CancellationToken.None);
        TestContext.WriteLine($"{handler.LastUri} -> {result.Position?.Latitude},{result.Position?.Longitude} {result.Quality}");

        Assert.AreEqual(37.4224, result.Position!.Latitude, 1e-9);
        Assert.AreEqual(-122.0842, result.Position.Longitude, 1e-9);
        Assert.AreEqual(ResultQuality.Rooftop, result.Quality);
        Assert.AreEqual("1600 Amphitheatre Pkwy, Mountain View, CA", result.NormalizedAddress);
        StringAssert.Contains(handler.LastUri!.AbsoluteUri, "address=1600%20Amphitheatre%20Pkwy%2C%20Mountain%20View%2C%20CA");
        StringAssert.Contains(handler.LastUri.AbsoluteUri, "key=k1");
    }

    [TestMethod]
    [DataRow("RANGE_INTERPOLATED", ResultQuality.Interpolated)]
    [DataRow("GEOMETRIC_CENTER", ResultQuality.Approximate)]
    [DataRow("APPROXIMATE", ResultQuality.Approximate)]
    public async Task Geocode_LocationType_MapsToQuality(string type, ResultQuality expected)
    {
        var (service, _) = Create(_ => Json(Rooftop.Replace("ROOFTOP", type, StringComparison.Ordinal)));

        var result = await service.GeocodeAsync(Address, CancellationToken.None);

        Assert.AreEqual(expected, result.Quality);
    }

    [TestMethod]
    public async Task Reverse_SendsLatLngWithInvariantCulture()
    {
        var (service, handler) = Create(_ => Json(Rooftop));

        var result = await service.ReverseGeocodeAsync(new GlobalPosition(37.4224, -122.0842), CancellationToken.None);

        Assert.AreEqual(ResultQuality.Rooftop, result.Quality);
        StringAssert.Contains(handler.LastUri!.AbsoluteUri, "latlng=37.4224,-122.0842");
    }

    [TestMethod]
    public async Task Geocode_NoKey_ReturnsNoMatchWithoutCalling()
    {
        var (service, handler) = Create(_ => Json(Rooftop), key: null);

        var result = await service.GeocodeAsync(Address, CancellationToken.None);

        Assert.AreEqual(ResultQuality.None, result.Quality);
        Assert.AreEqual(0, handler.Calls);
    }

    [TestMethod]
    [DataRow("""{"status":"ZERO_RESULTS","results":[]}""")]
    [DataRow("""{"status":"REQUEST_DENIED","results":[]}""")]
    [DataRow("""{"status":"OK","results":[]}""")]
    [DataRow("""{"status":"OK","results":[{"geometry":{}}]}""")]
    [DataRow("""{"status":"OK","results":[{"geometry":{"location":{"lat":99,"lng":0}}}]}""")]
    [DataRow("not json")]
    public async Task Geocode_BadOrEmptyBody_ReturnsNoMatch(string body)
    {
        var (service, _) = Create(_ => Json(body));

        var result = await service.GeocodeAsync(Address, CancellationToken.None);

        Assert.AreEqual(ResultQuality.None, result.Quality);
        Assert.IsNull(result.Position);
    }

    [TestMethod]
    public async Task Geocode_HttpErrorOrException_ReturnsNoMatch()
    {
        var (failing, _) = Create(_ => Json("{}", HttpStatusCode.Forbidden));
        var (throwing, _) = Create(_ => throw new HttpRequestException("down"));

        Assert.AreEqual(ResultQuality.None, (await failing.GeocodeAsync(Address, CancellationToken.None)).Quality);
        Assert.AreEqual(ResultQuality.None, (await throwing.GeocodeAsync(Address, CancellationToken.None)).Quality);
    }

    [TestMethod]
    public async Task TryAddGoogleGeocoding_ResolvesTheServiceWithTheConfiguredKey()
    {
        var services = new ServiceCollection();
        services.TryAddGoogleGeocoding(o => o.ApiKey = "abc");
        await using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<GoogleLocationServices>(provider.GetRequiredService<ILocationServices>());
        Assert.AreEqual("abc", provider.GetRequiredService<IOptions<GoogleMapsOptions>>().Value.ApiKey);
    }
}
