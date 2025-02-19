namespace OoBDev.AI.Models;

/// <summary>
/// Base level KeyValuePairModel class
/// </summary>
public class KeyValuePairModel
{
    /// <summary>
    /// Gets or sets the Key to be used.
    /// </summary>
    public required string Key { get; set; }

    /// <summary>
    /// Gets or sets the Value to be used.
    /// </summary>
    public required string Value { get; set; }
}
