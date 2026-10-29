using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.OData.Tests;

[TestClass]
public class ODataQueryTests
{
    private static async Task<(HttpStatusCode Status, JsonElement Body, HttpResponseMessage Response)> GetAsync(ODataHost host, string query)
    {
        var response = await host.Client.GetAsync("/items" + query);
        var text = await response.Content.ReadAsStringAsync();
        var body = (text.StartsWith('{') || text.StartsWith('[')) ? JsonDocument.Parse(text).RootElement.Clone() : default;
        return (response.StatusCode, body, response);
    }

    private static int Count(JsonElement body) => (body.ValueKind == JsonValueKind.Array ? body : body.GetProperty("value")).GetArrayLength();

    private static JsonElement Rows(JsonElement body) => body.ValueKind == JsonValueKind.Array ? body : body.GetProperty("value");

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_FilterOrderTopSkip_ShapesTheResult()
    {
        // Stage
        await using var host = await ODataHost.StartAsync();

        // Test
        var (status, body, _) = await GetAsync(host, "?$filter=Id gt 10 and Id le 40&$orderby=Id desc&$top=5&$skip=2");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, status);
        Assert.AreEqual(5, Count(body));
        Assert.AreEqual(38, Rows(body)[0].GetProperty("id").GetInt32());
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_SelectAndCount_ReturnsOnlyTheColumnsAndTheTotal()
    {
        // Stage
        await using var host = await ODataHost.StartAsync();

        // Test
        var (status, body, _) = await GetAsync(host, "?$select=Id&$count=true&$top=3");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, status);
        Assert.AreEqual(200, body.GetProperty("@odata.count").GetInt32());
        Assert.IsFalse(Rows(body)[0].TryGetProperty("Name", out _));
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_NoTop_ReturnsTheDefaultPage()
    {
        // Stage
        await using var host = await ODataHost.StartAsync();

        // Test
        var (_, body, _) = await GetAsync(host, string.Empty);

        // Assert
        Assert.AreEqual(50, Count(body));
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_TopOverTheCap_IsAProblem()
    {
        // Stage
        await using var host = await ODataHost.StartAsync();

        // Test
        var (status, _, response) = await GetAsync(host, "?$top=101");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, status);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    [DataRow("?$apply=groupby((Name))")]
    [DataRow("?$expand=Foo")]
    [DataRow("?$filter=Id eq ")]
    [DataRow("?$filter=Nope eq 1")]
    [DataRow("?$filter=Secret eq 's1'")]
    [DataRow("?$orderby=Rank")]
    public async Task Get_DisallowedOrBadOption_IsABadRequest(string query)
    {
        // Stage
        await using var host = await ODataHost.StartAsync();

        // Test
        var (status, _, _) = await GetAsync(host, query);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, status);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_OverComplexFilter_IsABadRequest()
    {
        // Stage
        await using var host = await ODataHost.StartAsync(new Dictionary<string, string?> { ["OoBDev:OData:MaxNodeCount"] = "5" });

        // Test
        var (status, _, _) = await GetAsync(host, "?$filter=Id eq 1 or Id eq 2 or Id eq 3 or Id eq 4");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, status);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_Response_AdvertisesQuerySupport()
    {
        // Stage
        await using var host = await ODataHost.StartAsync();

        // Test
        var (_, _, response) = await GetAsync(host, "?$top=1");

        // Assert
        Assert.AreEqual("\"application/x-www-form-urlencoded\"", string.Join(",", response.Headers.GetValues("Accept-Query")));
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Query_FormBody_AppliesTheOptions()
    {
        // Stage
        await using var host = await ODataHost.StartAsync();
        using var request = new HttpRequestMessage(new HttpMethod("QUERY"), "/items")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["$filter"] = "Id le 3", ["$orderby"] = "Id desc", ["ignored"] = "x" }),
        };

        // Test
        using var response = await host.Client.SendAsync(request);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(3, Count(body));
        Assert.AreEqual(3, Rows(body)[0].GetProperty("id").GetInt32());
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Post_WithOverrideHeader_AppliesTheOptionsAndTheQueryStringWins()
    {
        // Stage
        await using var host = await ODataHost.StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/items?$top=2")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["$top"] = "9", ["$orderby"] = "Id" }),
        };
        request.Headers.Add("X-HTTP-Method-Override", "QUERY");

        // Test
        using var response = await host.Client.SendAsync(request);
        var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        // Assert
        Assert.AreEqual(2, Count(body));
    }
}
