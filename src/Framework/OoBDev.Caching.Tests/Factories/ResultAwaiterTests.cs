using OoBDev.TestUtilities;
using OoBDev.Caching.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Factories;

[TestClass]
public class ResultAwaiterTests
{
    public TestContext TestContext { get; set; }

    private MockRepository mockRepository;

    [TestInitialize]
    public void TestInitialize()
    {
        this.mockRepository = new MockRepository(MockBehavior.Strict);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void UnwrapTest()
    {
        // Stage
        var testValue = 23452;
        object task = Task.FromResult(testValue);

        // Mock

        // Test

        var result = ResultAwaiter.Unwrap(typeof(int), task);

        // Assert
        Assert.AreEqual(testValue, result);

        // Verify
        this.mockRepository.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task WrapTest()
    {
        // Stage
        object input = 234556;

        // Mock

        // Test
        var result = ResultAwaiter.Wrap(input);
        var taskResult = await (Task<int>)result;

        // Assert
        Assert.AreEqual(input, taskResult);

        // Verify
        this.mockRepository.VerifyAll();
    }
}
