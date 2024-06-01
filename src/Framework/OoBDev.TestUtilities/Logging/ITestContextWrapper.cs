using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Logging;

public interface ITestContextWrapper
{
    TestContext Context { get; }
}
