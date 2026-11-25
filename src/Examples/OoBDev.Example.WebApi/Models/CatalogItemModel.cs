namespace OoBDev.WebApi.Models;

/// <summary>A row of the sample catalog served through the OData query surface.</summary>
/// <param name="Id">The row number.</param>
/// <param name="Name">The item name.</param>
/// <param name="Category">The category.</param>
/// <param name="Price">The price.</param>
/// <param name="InStock">Whether the item is in stock.</param>
public sealed record CatalogItemModel(int Id, string Name, string Category, decimal Price, bool InStock);
