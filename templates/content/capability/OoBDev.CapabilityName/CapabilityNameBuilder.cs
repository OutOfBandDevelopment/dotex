namespace OoBDev.CapabilityName;

/// <summary>
/// Overrides for <see cref="ServiceCollectionExtensions.TryAddCapabilityNameServices"/>.
/// </summary>
public record CapabilityNameBuilder
{
    /// <summary>
    /// Configuration section bound to <see cref="CapabilityNameOptions"/>.
    /// </summary>
    public string OptionsSection { get; init; } = nameof(CapabilityNameOptions);
}
