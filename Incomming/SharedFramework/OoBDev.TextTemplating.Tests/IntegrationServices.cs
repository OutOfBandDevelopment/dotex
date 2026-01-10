using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TextTemplating.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetSimulateServices() => new ServiceCollection()
            .AddDebugTestConfigurations(
                  ("ApplicationCoreDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;")
            )
            .AddTextTemplatingServices()
            .AddToolkitServices()
            ;

        public static T GetSimuateService<T>(this TestContext context) => GetSimulateServices()
            .AddTestLoggingServices(context)
            .BuildServiceProvider()
            .GetService<T>()
            ;
    }
}