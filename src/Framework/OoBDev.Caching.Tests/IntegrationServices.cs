using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Caching.Tests.Providers;
using OoBDev.TestUtilities.Logging;

namespace OoBDev.Caching.Tests;

public static class IntegrationServices
{
    public static IServiceCollection GetServices(this TestContext testContext)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection()
            .Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
        //    .AddDebugTestConfigurations()
            .AddTestLoggingServices(testContext)
            .TryAddCachingServices()
            .AddSingleton<ICachingProvider, NullCachingProvider>() // Default no-op provider for tests
            //.AddMicrosoftCachingServices()
            //.AddRedisCachingServices()
            //.AddToolkitServices()
            ;
    }


    public static T GetService<T>(this TestContext testContext, IServiceCollection services = null) =>
        (services ?? testContext.GetServices())
            .BuildServiceProvider()
            .GetService<T>()
            ;

}
