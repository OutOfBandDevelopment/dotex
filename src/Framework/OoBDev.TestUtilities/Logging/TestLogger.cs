using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace OoBDev.TestUtilities.Logging;

/// <summary>
/// Logger implementation that writes log messages to MSTest TestContext output.
/// </summary>
public class TestLogger : ILogger
{
    /// <summary>
    /// The TestContext to write log messages to.
    /// </summary>
    protected readonly TestContext _context;

    /// <summary>
    /// The logger category name.
    /// </summary>
    protected readonly string? _category;

    /// <summary>
    /// Initializes a new instance of the TestLogger class with a TestContext.
    /// </summary>
    /// <param name="testContext">The test context to write logs to.</param>
    /// <param name="category">Optional category name for the logger.</param>
    public TestLogger(
        TestContext testContext,
        string? category = null
        )
    {
        _context = testContext;
        _category = string.IsNullOrWhiteSpace(category) ? null : category;
    }

    /// <summary>
    /// Initializes a new instance of the TestLogger class with a wrapped TestContext.
    /// </summary>
    /// <param name="contextWrapper">The wrapped test context to write logs to.</param>
    /// <param name="category">Optional category name for the logger.</param>
    public TestLogger(
        ITestContextWrapper contextWrapper,
        string? category = null
        )
    {
        _context = contextWrapper.Context;
        _category = string.IsNullOrWhiteSpace(category) ? null : category;
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>A disposable object that ends the scope when disposed.</returns>
    public virtual IDisposable? BeginScope<TState>(TState state) where TState : notnull => new LoggerScope<TState>(state);

    /// <summary>
    /// Checks if the given log level is enabled.
    /// </summary>
    /// <param name="logLevel">The log level to check.</param>
    /// <returns>True (all log levels are enabled for test logging).</returns>
    public virtual bool IsEnabled(LogLevel logLevel) => true;

    /// <summary>
    /// Writes a log entry to the test context output.
    /// </summary>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a message of the state and exception.</param>
    public virtual void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        void WriteMessage(string message)
        {
            if (_context == null)
            {
                Debug.WriteLine(message);
            }
            else
            {
                _context.WriteLine(message);
            }
        }

        if (formatter != null)
        {
            WriteMessage($@"{_category}-LOG>{logLevel}({eventId}): {formatter(state, exception)}");
        }
        else
        {
            WriteMessage($@"{_category}-LOG>{logLevel}({eventId}): {state}");
            if (exception != null)
            {
                WriteMessage($@"{_category}-ERROR>{logLevel}({eventId}): {exception}");
            }
        }
    }
}

/// <summary>
/// Generic logger implementation that writes log messages to MSTest TestContext output with a specific category type.
/// </summary>
/// <typeparam name="T">The type used as the logger category name.</typeparam>
public class TestLogger<T> : TestLogger, ILogger<T>
{
    /// <summary>
    /// Initializes a new instance of the TestLogger&lt;T&gt; class with a TestContext.
    /// </summary>
    /// <param name="testContext">The test context to write logs to.</param>
    public TestLogger(
        TestContext testContext
        ) : base(testContext, typeof(T).FullName ?? throw new InvalidOperationException($"This shouldn't be possible"))
    {
    }

    /// <summary>
    /// Initializes a new instance of the TestLogger&lt;T&gt; class with a wrapped TestContext.
    /// </summary>
    /// <param name="contextWrapper">The wrapped test context to write logs to.</param>
    public TestLogger(
        ITestContextWrapper contextWrapper
        ) : base(contextWrapper, typeof(T).FullName ?? throw new InvalidOperationException($"This shouldn't be possible"))
    {
    }
}
