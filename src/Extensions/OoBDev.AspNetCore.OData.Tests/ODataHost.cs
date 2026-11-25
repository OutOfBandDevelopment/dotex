using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.AspNetCore.Mvc;
using OoBDev.System.ComponentModel.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.OData.Tests;

public sealed class Item
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    [NotFilterable]
    public string Secret { get; set; } = string.Empty;
    [NotSortable]
    public int Rank { get; set; }
}

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private static readonly List<Item> _items = Enumerable.Range(1, 200)
        .Select(i => new Item { Id = i, Name = $"Item{i:000}", Secret = $"s{i}", Rank = i % 7 })
        .ToList();

    [HttpGet]
    [OoBDevEnableQuery]
    public IQueryable<Item> Get() => _items.AsQueryable();
}

public sealed class ODataHost : IAsyncDisposable
{
    private readonly WebApplication _app;

    private ODataHost(WebApplication app, HttpClient client)
    {
        _app = app;
        Client = client;
    }

    public HttpClient Client { get; }

    public static async Task<ODataHost> StartAsync(Dictionary<string, string?>? settings = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(settings ?? []);
        builder.Services.TryAddODataQueryServices(builder.Configuration, ServiceCollectionExtensions.DefaultSection);
        builder.Services.AddControllers().AddApplicationPart(typeof(ItemsController).Assembly);
        var app = builder.Build();
        app.UseHttpQueryVerb();
        app.UseODataQueryBody();
        app.UseRouting();
        app.MapControllers();
        await app.StartAsync().ConfigureAwait(false);
        var address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        return new ODataHost(app, new HttpClient { BaseAddress = new Uri(address) });
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await _app.DisposeAsync().ConfigureAwait(false);
    }
}
