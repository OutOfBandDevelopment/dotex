namespace OoBDev.System.Utilities;

/// <summary>
/// Represents a selected service instance from a collection of available services.
/// </summary>
/// <typeparam name="TService">The type of the service.</typeparam>
public interface ISelectedService<TService> where TService : notnull
{
    /// <summary>
    /// Gets the selected service instance.
    /// </summary>
    TService Value { get; }
}
