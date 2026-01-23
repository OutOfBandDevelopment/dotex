using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Logging;

/// <summary>
/// Implements ITestContextWrapper to wrap a TestContext for dependency injection.
/// </summary>
/// <param name="context">The TestContext to wrap.</param>
public class TestContextWrapper(TestContext context) : ITestContextWrapper
{
    /// <summary>
    /// Gets the wrapped TestContext instance.
    /// </summary>
    public TestContext Context { get; } = context;
}
