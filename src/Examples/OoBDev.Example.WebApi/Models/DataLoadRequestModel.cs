namespace OoBDev.WebApi.Models;

/// <summary>A small file to load into a throwaway in-memory table.</summary>
/// <param name="Format">The reader format, <c>csv</c> or <c>json</c>.</param>
/// <param name="Text">The file text.</param>
/// <param name="KeyColumn">The column that identifies a row; a repeated key is a duplicate.</param>
/// <param name="Duplicates">What to do with a duplicate: <c>Skip</c>, <c>Replace</c> or <c>Fail</c>.</param>
public sealed record DataLoadRequestModel(string Format, string Text, string KeyColumn, string Duplicates = "Skip");
