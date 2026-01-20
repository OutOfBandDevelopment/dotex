using OoBDev.Microsoft.Caching;
using OoBDev.Redis.Caching;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Caching.Common.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices(this TestContext testContext) =>
            new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddTestLoggingServices(testContext)
                .AddOoBDevCachingServices()
                .AddMicrosoftCachingServices()
                .AddRedisCachingServices()
                .AddToolkitServices()
                ;


        public static T GetService<T>(this TestContext testContext, IServiceCollection services = null) =>
            (services ?? testContext.GetServices())
                .BuildServiceProvider()
                .GetService<T>()
                ;

    }
}