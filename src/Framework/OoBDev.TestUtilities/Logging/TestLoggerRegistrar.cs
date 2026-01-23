using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Logging;

/// <summary>
/// Provides extension methods for registering test logging services.
/// </summary>
public static class TestLoggerRegistrar
{
    /// <summary>
    /// Adds test logging services to the service collection.
    /// Registers TestContext wrapper, logger factory, and test loggers.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="context">The test context to use for logging.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddTestLoggingServices(this IServiceCollection services, TestContext context) =>
        services
            .AddTransient<ITestContextWrapper>(sp => new TestContextWrapper(context))
            .AddSingleton(sp => LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddDebug()))
            .AddSingleton<ILogger, TestLogger>()
            .AddSingleton(typeof(ILogger<>), typeof(TestLogger<>))
        ;
}
