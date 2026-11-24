namespace OoBDev.AspNetCore.Mvc;

/// <summary>The body of a screen view call.</summary>
/// <param name="Screen">The screen name.</param>
public sealed record ScreenViewRequest(string? Screen);
