using OoBDev.System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OoBDev.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.AspNetCore.Mvc.OpenApi;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.OpenApi;

/// <summary>A model that is exposed through a searchable endpoint.</summary>
public record Item(int Id, string Name);

/// <summary>Items endpoints used to verify the document transformers.</summary>
[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    /// <summary>Gets the items.</summary>
    /// <param name="hint">A hint.</param>
    [HttpGet]
    [ApplicationRight("Items.Read")]
    public IQueryable<Item> Get(string? hint = null) => new[] { new Item(1, "one") }.AsQueryable();

    /// <summary>Anyone can ping.</summary>
    [HttpGet("ping")]
    [AllowAnonymous]
    public string Ping() => "pong";
}

[TestClass]
[TestCategory(TestCategories.Simulate)]
public class OpenApiDocumentTests
{
    private static WebApplication? _app;
    private static HttpClient? _client;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext _)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        var services = builder.Services;
        services.TryAddSystemExtensions(builder.Configuration, new SystemExtensionBuilder());
        services.AddControllers().AddApplicationPart(typeof(ItemsController).Assembly);
        services.TryAddCommonOpenApiExtensions([typeof(ItemsController).Assembly]);
        services.TryAddAspNetCoreSearchQuery();

        _app = builder.Build();
        _app.MapControllers();
        _app.MapOpenApi();
        await _app.StartAsync();

        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        _client = new HttpClient { BaseAddress = new Uri(address) };
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        _client?.Dispose();
        if (_app != null) await _app.DisposeAsync();
    }

    private static async Task<JsonNode> GetDocumentAsync(string name) =>
        JsonNode.Parse(await _client!.GetStringAsync($"/openapi/{name}.json"))!;

    [TestMethod]
    public void Catalog_ListsAllAndAssemblyDocuments()
    {
        var catalog = _app!.Services.GetRequiredService<OpenApiDocumentCatalog>();

        CollectionAssert.AreEqual(new[] { "all", typeof(ItemsController).Assembly.GetName().Name }, catalog.Names.ToArray());
    }

    [TestMethod]
    public async Task Document_IncludesHealthAndPermissions()
    {
        var document = await GetDocumentAsync("all");

        Assert.IsNotNull(document["paths"]!["/health"]);
        Assert.IsNotNull(document["paths"]!["/api/items"]); var items = document["paths"]!["/api/items"]!["get"]!;
        Assert.AreEqual(JsonValueKind.Object, items["x-permissions"]!.GetValueKind());
        var ping = document["paths"]!["/api/items/ping"]!["get"]!;
        Assert.IsTrue(ping["x-permissions"]!.ToJsonString().Contains("true", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public async Task Document_AssemblyDocumentOnlyHasItsControllers()
    {
        var name = typeof(ItemsController).Assembly.GetName().Name!;

        var document = await GetDocumentAsync(name);

        Assert.AreEqual(name, (string?)document["info"]!["title"]);
        Assert.IsNotNull(document["paths"]!["/api/items"]);
    }

    [TestMethod]
    public async Task Document_SearchQueryEndpoint_DescribesSortingAndPagedResponse()
    {
        var document = await GetDocumentAsync("all");

        var operation = document["paths"]!["/api/items"]!["get"]!;
        var tags = operation["tags"]!.AsArray().Select(t => (string?)t).ToList();
        CollectionAssert.Contains(tags, "IQueryable");

        var parameters = operation["parameters"]!.AsArray().Select(p => (string?)p!["name"]).ToList();
        Assert.IsTrue(parameters.Any(p => p!.StartsWith("orderBy.", StringComparison.OrdinalIgnoreCase)), string.Join(",", parameters));
        CollectionAssert.Contains(parameters, "pageSize");

        var schema = operation["responses"]!["200"]!["content"]!["application/json"]!["schema"]!;
        Assert.IsNotNull(schema["properties"]!["rows"], schema.ToJsonString());
        Assert.IsNotNull(schema["properties"]!["totalRowCount"]);
        Assert.AreEqual("Gets the items.", (string?)operation["summary"]);
    }
}
