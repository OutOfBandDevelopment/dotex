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
    /// Integration tests run against Docker-based external services (databases, message queues,
    /// search engines, etc.). Requires Docker containers to be running locally or in CI/CD.
    /// Runs in daily integration test pipeline.
    /// </summary>
    public const string Integration = nameof(Integration);

    /// <summary>
    /// Test points for local development only. Not expected to be safe to rerun and may use
    /// persisted resources. Typically used for manual testing, performance benchmarks, or
    /// exploratory testing. Not executed in CI/CD pipelines.
    /// </summary>
    public const string DevLocal = nameof(DevLocal);

    /// <summary>
    /// Tests that require live cloud services (Azure B2C, Application Insights, Groq Cloud, etc.)
    /// which cannot be emulated or containerized locally. Requires valid cloud credentials and
    /// active service subscriptions. Manual execution only, not run in CI/CD pipelines.
    /// </summary>
    public const string LiveIntegration = nameof(LiveIntegration);
}
