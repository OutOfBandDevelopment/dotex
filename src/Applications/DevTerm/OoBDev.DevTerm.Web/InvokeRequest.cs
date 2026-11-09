namespace OoBDev.DevTerm.Web;

/// <summary>Body of <c>POST /api/invoke</c>: a control panel command id and its string value (see <see cref="OoBDev.UiDefinitions.IControlSurface"/>).</summary>
public sealed record InvokeRequest(string CommandId, string? Value);
