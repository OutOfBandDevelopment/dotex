namespace OoBDev.WebApi.Models;

/// <summary>A row that did not load.</summary>
/// <param name="Line">The source line.</param>
/// <param name="Message">The reason.</param>
public sealed record DataLoadErrorModel(int Line, string Message);
