using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities.Logging;

namespace OoBDev.Caching.Tests;

public static class IntegrationServices
{
    public static IServiceCollection GetServices(this TestContext testContext) =>
        new ServiceCollection()
        //    .AddDebugTestConfigurations()
            .AddTestLoggingServices(testContext)
            .AddCachingServices()
            //.AddMicrosoftCachingServices()
            //.AddRedisCachingServices()
            //.AddToolkitServices()
            ;


    public static T GetService<T>(this TestContext testContext, IServiceCollection services = null) =>
        (services ?? testContext.GetServices())
            .BuildServiceProvider()
            .GetService<T>()
            ;

}
