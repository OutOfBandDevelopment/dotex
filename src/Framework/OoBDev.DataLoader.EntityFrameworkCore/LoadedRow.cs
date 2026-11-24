namespace OoBDev.DataLoader.EntityFrameworkCore;

/// <summary>A stored row: the dataset and key identify it, the values are kept as JSON.</summary>
public sealed class LoadedRow
{
    /// <summary>Dataset name, so one table holds many loads.</summary>
    public string Dataset { get; set; } = string.Empty;

    /// <summary>Key column value.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Column values as a JSON object.</summary>
    public string ValuesJson { get; set; } = "{}";
}
