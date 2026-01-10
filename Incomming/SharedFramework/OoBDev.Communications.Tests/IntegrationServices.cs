using OoBDev.ComplexEvents.Common;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Communications.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices(this TestContext testContext) => new ServiceCollection()
            .AddDebugTestConfigurations(
                ("Azure:Storage:Default:ConnectionString", "UseDevelopmentStorage=true"),
                ("ApplicationCoreDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),
                ("ApplicationReadDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),

                // TODO: Configure via user secrets or environment variables
                // See: https://docs.microsoft.com/aspnet/core/security/app-secrets
                ("Twilio:SmsMessaging:AuthToken", Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN") ?? "[YOUR_TWILIO_AUTH_TOKEN]"),
                ("Twilio:SmsMessaging:AccountSid", Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID") ?? "[YOUR_TWILIO_ACCOUNT_SID]"),
                ("Twilio:SmsMessaging:Default:From", Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER") ?? "+15005550006"),

                ("Twilio:SendGrid:ApiKey", Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? "[YOUR_SENDGRID_API_KEY]"),
                ("Twilio:SendGrid:Default:From:Email", "no-reply@oobdev.com"),
                ("Twilio:SendGrid:Default:From:Name", "OoBDev Dev"),
                ("Twilio:SendGrid:Default:Subject", "Hello from OoBDev")
            )
            .AddCommunicationsServices()
            .AddTestLoggingServices(testContext)
            .AddToolkitServices()
            .AddComplexEventsServices()
            ;

        public static T GetService<T>(this TestContext testContext, IServiceCollection services = null) => (services ?? testContext.GetServices())
            .BuildServiceProvider()
            .GetService<T>()
            ;
    }
}