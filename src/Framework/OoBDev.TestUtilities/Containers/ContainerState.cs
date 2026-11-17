namespace OoBDev.TestUtilities.Containers;

/// <summary>What <c>docker inspect</c> reports about a container.</summary>
/// <param name="Running">The container process is running.</param>
/// <param name="Health">The health status (<c>healthy</c>, <c>starting</c>, <c>unhealthy</c>), or empty when the image has no health check.</param>
public sealed record ContainerState(bool Running, string Health)
{
    /// <summary>Running and, when a health check exists, healthy.</summary>
    public bool Ready => Running && (Health.Length == 0 || Health == "healthy");
}
