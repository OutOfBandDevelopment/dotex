using OoBDev.Microsoft.Azure.Storage;
using OoBDev.TestUtilities;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Twilio.SmsMessaging.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices(this TestContext context) => new ServiceCollection()
            .AddDebugTestConfigurations(
                // TODO: Configure via user secrets or environment variables
                // See: https://docs.microsoft.com/aspnet/core/security/app-secrets
                ("Twilio:SmsMessaging:AuthToken", Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? "[YOUR_TWILIO_AUTH_TOKEN]"),
                ("Twilio:SmsMessaging:AccountSid", Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID") ?? "[YOUR_TWILIO_ACCOUNT_SID]"),
                ("Twilio:SmsMessaging:Default:From", Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER") ?? "+15005550006"),

                ("Azure:Storage:Default:ConnectionString", "UseDevelopmentStorage=true")
            )
            .AddDebugTestServices(context)
            .AddTwilioSmsServices()
            .AddAzureStorageServices()
            .AddToolkitServices()
            ;

        public static T GetService<T>(this TestContext context, IServiceCollection services = null) => (services ?? GetServices(context))
            .BuildServiceProvider()
            .GetService<T>()
            ;
    }
}