using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.AspNetCore.OData;
using OoBDev.WebApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.WebApi.Controllers;

/// <summary>A small in-memory catalog that shows the OData system query options (<c>$filter</c>, <c>$orderby</c>, <c>$top</c>, <c>$skip</c>, <c>$select</c>, <c>$count</c>) over HTTP <c>GET</c> and <c>QUERY</c>.</summary>
[ApiController]
[Route("api/[controller]")]
public class SampleCatalogController : ControllerBase
{
    private static readonly List<CatalogItemModel> _items =
    [
        new(1, "Resistor kit", "Parts", 9.50m, true),
        new(2, "Capacitor kit", "Parts", 11.25m, true),
        new(3, "Breadboard", "Tools", 6.00m, true),
        new(4, "Soldering iron", "Tools", 24.99m, false),
        new(5, "Multimeter", "Instruments", 39.00m, true),
        new(6, "Oscilloscope", "Instruments", 329.00m, false),
        new(7, "Logic analyzer", "Instruments", 59.00m, true),
        new(8, "Jumper wires", "Parts", 4.75m, true),
        new(9, "Heat gun", "Tools", 32.40m, true),
        new(10, "Bench supply", "Instruments", 119.00m, true),
    ];

    /// <summary>Queries the catalog with the OData system query options.</summary>
    /// <returns>The catalog as a queryable; the filter shapes what is returned.</returns>
    [HttpGet]
    [ApplicationRight("catalog.read")]
    [OoBDevEnableQuery]
    [ProducesResponseType<IEnumerable<CatalogItemModel>>(StatusCodes.Status200OK)]
    public IQueryable<CatalogItemModel> Query() => _items.AsQueryable();
}
