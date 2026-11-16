using System.Text.Json;

namespace OoBDev.Query.Metadata;

/// <summary>
/// What a keyset cursor holds: where it belongs and the sort values of the last row of the page.
/// </summary>
/// <param name="Kind">The kind the cursor was issued for.</param>
/// <param name="Sort">The normalized sort it was issued for.</param>
/// <param name="Values">The sort column values of the last row, the key last.</param>
public sealed record CursorState(string Kind, string Sort, JsonElement[] Values);
