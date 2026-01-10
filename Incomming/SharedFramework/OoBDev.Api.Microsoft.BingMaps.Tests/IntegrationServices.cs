using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Communications;
using OoBDev.TestUtilities.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Api.Microsoft.BingMaps.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices() => new ServiceCollection()
            .AddDebugTestConfigurations(
                ("Microsoft:BingMaps:ApiKey", "Aq_xgo3-Ngout0eM7OmI9VYRxuI6BgiyFe6ywEG1ZY0tRcuhqvQv7Vr1FI5ivP5V")
            )
            ;

        public static T GetService<T>(IServiceCollection services = null, TestContext context = null) => (services ?? GetServices())
            .AddTestLoggingServices(context)
            .AddDebugCommunication(context)
            .BuildServiceProvider()
            .GetService<T>()
            ;
    }
}