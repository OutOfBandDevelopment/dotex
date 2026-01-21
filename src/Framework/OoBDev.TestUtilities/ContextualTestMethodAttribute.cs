using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.TestUtilities;

/// <summary>
/// MSTest attribute that provides async-local context for the currently executing test method and test instance.
/// </summary>
/// <remarks>
/// This attribute extends <see cref="TestMethodAttribute"/> to automatically capture and provide access
/// to the current test context using <see cref="AsyncLocal{T}"/>, ensuring thread-safe access to test
/// metadata during test execution. This is particularly useful for helper methods, utilities, or logging
/// that need to access test information without explicit parameter passing.
/// </remarks>
/// <example>
/// <code>
/// [TestClass]
/// public class MyTests
/// {
///     [ContextualTestMethod]
///     public void MyTest()
///     {
///         // Access current test method anywhere in the call stack
///         var testName = ContextualTestMethodAttribute.Current?.TestMethodName;
///
///         // Access current test instance
///         var instance = ContextualTestMethodAttribute.Instance;
///     }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method)]
public class ContextualTestMethodAttribute : TestMethodAttribute
{
    /// <summary>
    /// Constant key for the current test method context.
    /// </summary>
    public const string CurrentTestMethod = nameof(CurrentTestMethod);

    /// <summary>
    /// Constant key for the current test instance context.
    /// </summary>
    public const string CurrentTestInstance = nameof(CurrentTestInstance);

    private readonly static AsyncLocal<ITestMethod?> _current = new();
    private readonly static AsyncLocal<object?> _instance = new();

    /// <summary>
    /// Gets the currently executing test method, if any.
    /// </summary>
    /// <value>
    /// The <see cref="ITestMethod"/> representing the currently executing test, or <c>null</c> if no test is running.
    /// </value>
    /// <remarks>
    /// This property uses <see cref="AsyncLocal{T}"/> to ensure thread-safe access across async operations
    /// and provides the test context for the duration of test execution.
    /// </remarks>
    public static ITestMethod? Current => _current.Value;

    /// <summary>
    /// Gets or sets the current test instance.
    /// </summary>
    /// <value>
    /// The test class instance currently executing, or <c>null</c> if no test is running.
    /// </value>
    /// <remarks>
    /// This property uses <see cref="AsyncLocal{T}"/> to ensure thread-safe access across async operations.
    /// The instance is typically the test class object (decorated with <see cref="TestClassAttribute"/>).
    /// </remarks>
    public static object? Instance
    {
        get => _instance.Value;
        set => _instance.Value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualTestMethodAttribute"/> class.
    /// </summary>
    public ContextualTestMethodAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContextualTestMethodAttribute"/> class with a display name.
    /// </summary>
    /// <param name="displayName">The display name for the test method.</param>
    public ContextualTestMethodAttribute(string? displayName) : base()
    {
    }

    /// <summary>
    /// Executes the test method asynchronously while maintaining the test context.
    /// </summary>
    /// <param name="testMethod">The test method to execute.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing an array of <see cref="TestResult"/>
    /// objects representing the outcome of the test execution.
    /// </returns>
    /// <remarks>
    /// This method sets the <see cref="Current"/> property before executing the test and clears it
    /// after execution completes, ensuring proper context lifecycle management.
    /// </remarks>
    public override async Task<TestResult[]> ExecuteAsync(ITestMethod testMethod)
    {
        _current.Value = testMethod;
        var ret = await base.ExecuteAsync(testMethod);
        _current.Value = null;
        return ret;
    }
}
