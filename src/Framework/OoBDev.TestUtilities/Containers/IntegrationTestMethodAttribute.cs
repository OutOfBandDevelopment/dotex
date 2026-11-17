using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace OoBDev.TestUtilities.Containers;

/// <summary>
/// A <c>[TestMethod]</c> for an Integration test that needs containers of the test stack: before the test runs the
/// containers are checked, started when they are down and awaited until healthy (<see cref="IntegrationContainers"/>),
/// and the test ends Inconclusive when they do not come up in time.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class IntegrationTestMethodAttribute : TestMethodAttribute
{
    private readonly string[] _containers;

    /// <summary>Creates the attribute.</summary>
    /// <param name="containers">Container names of the integration stack separated by commas, for example <c>oobd-test-redis</c>.</param>
    /// <param name="callerFilePath">Filled in by the compiler.</param>
    /// <param name="callerLineNumber">Filled in by the compiler.</param>
    public IntegrationTestMethodAttribute(string containers, [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNumber = -1)
        : base(callerFilePath, callerLineNumber) =>
        _containers = containers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <inheritdoc />
    public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        try
        {
#pragma warning disable MSTESTEXP
            await IntegrationContainers.EnsureAsync(TestContext.Current!, _containers);
#pragma warning restore MSTESTEXP
        }
        catch (AssertInconclusiveException ex)
        {
            return [new TestResult { Outcome = UnitTestOutcome.Inconclusive, TestFailureException = ex }];
        }

        return await base.ExecuteAsync(testMethod);
    }
}
