using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Logging;

/// <summary>
/// Wraps a TestContext instance for dependency injection scenarios.
/// </summary>
public interface ITestContextWrapper
{
    /// <summary>
    /// Gets the wrapped TestContext instance.
    /// </summary>
    TestContext Context { get; }
}
