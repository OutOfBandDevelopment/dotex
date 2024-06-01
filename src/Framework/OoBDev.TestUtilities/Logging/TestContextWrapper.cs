using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Logging;

public class TestContextWrapper(TestContext context) : ITestContextWrapper
{
    public TestContext Context { get; } = context;
}
