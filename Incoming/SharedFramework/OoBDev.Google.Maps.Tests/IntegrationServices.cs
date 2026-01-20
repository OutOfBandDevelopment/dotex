#nullable enable

using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Communications;
using OoBDev.TestUtilities.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Google.Maps.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices() => new ServiceCollection()
            .AddDebugTestConfigurations(
                    ("Google:Maps:ApiKey", "AIzaSyDog_oYXCQ0m5ZpEXhAEYcsZ87a7p5t0nc")
            )
            ;

        public static T GetService<T>(this TestContext context, IServiceCollection? services = null) => (services ?? GetServices())
                .AddTestLoggingServices(context)
                .AddDebugCommunication(context)
                .BuildServiceProvider()
                .GetService<T>()
                ;
    }
}