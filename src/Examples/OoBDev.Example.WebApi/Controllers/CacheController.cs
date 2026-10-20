using OoBDev.Caching;
using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// A small playground for the caching provider selected by configuration: store, look up and flush text by key.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CacheController(ICachingProvider cache) : ControllerBase
{
    /// <summary>Stores text under a key for a number of seconds.</summary>
    /// <param name="model">The key, text and lifetime.</param>
    /// <returns>No content.</returns>
    [HttpPut]
    public async Task<IActionResult> Store([FromBody] CacheStoreModel model)
    {
        await cache.StoreAsync(model.Key, model.Value, TimeSpan.FromSeconds(Math.Clamp(model.Seconds, 1, 3600)));
        return NoContent();
    }

    /// <summary>Looks a key up.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>Whether it was found and the text.</returns>
    [HttpGet]
    public async Task<CacheLookupModel> Lookup(string key)
    {
        var value = await cache.RetrieveAsync(key, typeof(string));
        return new() { Key = key, Found = value is not null, Value = value as string, Provider = cache.GetType().Name };
    }

    /// <summary>Removes a key.</summary>
    /// <param name="key">The cache key.</param>
    /// <returns>No content.</returns>
    [HttpDelete]
    public async Task<IActionResult> Flush(string key)
    {
        await cache.FlushAsync(key);
        return NoContent();
    }
}
