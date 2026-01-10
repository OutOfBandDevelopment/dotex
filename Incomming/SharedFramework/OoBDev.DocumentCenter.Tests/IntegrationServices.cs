using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DocumentCenter.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices(this TestContext testContext) => new ServiceCollection()
            .AddDebugTestConfigurations(
                ("Azure:Storage:Default:ConnectionString", "UseDevelopmentStorage=true"),

                ("ApplicationCoreDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),
                ("ApplicationReadDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),

                ("RestpackIo:AccessToken", "95wuOrHAIG8fELQUJWNffuxBCytz616WVif5PgBRA7K9Ydj4"),
                ("DocRaptor:ApiKey", "YOUR_API_KEY_HERE")
            )
            .AddTestLoggingServices(testContext)
            .AddDocumentCenterServices()
            .AddToolkitServices()
            .AddMessageQueueingServices()
            ;

        public static T GetService<T>(this TestContext testContext, IServiceCollection services = null) => (services ?? testContext.GetServices())
            .BuildServiceProvider()
            .GetService<T>()
            ;
    }
}