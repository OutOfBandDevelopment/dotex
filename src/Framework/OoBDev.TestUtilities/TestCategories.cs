namespace OoBDev.TestUtilities;

/// <summary>
/// Common test categories
/// </summary>
public static class TestCategories
{
    /// <summary>
    /// Unit tests are rerun-able, standalone tests for a single operation.  External resources should be 
    /// mocked out so these are fast and may run within a pipeline.
    /// </summary>
    public const string Unit = nameof(Unit);

    /// <summary>
    /// Simulation tests are similar to integration tests by testing the majority of the software stack.
    /// The difference being Simulations use mocked entry and persist layers so they may be executed within
    /// a pipeline without requiring external resources. 
    /// </summary>
    public const string Simulate = nameof(Simulate);

    /// <summary>
    /// Integration tests should support the ability to run against deployed environments
    /// including interacting with databases and web services
    /// </summary>
    public const string Integration = nameof(Integration);

    /// <summary>
    /// Test points for local development, not expected to be safe to return and may use persisted resources
    /// </summary>
    public const string DevLocal = nameof(DevLocal);
}
