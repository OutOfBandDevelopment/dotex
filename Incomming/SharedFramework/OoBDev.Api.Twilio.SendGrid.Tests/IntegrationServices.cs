using OoBDev.Azure.Storage;
using OoBDev.Communications;
using OoBDev.ComplexEvents.Common;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Api.Twilio.SendGrid.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices(this TestContext context) =>  new ServiceCollection()
            .AddDebugTestConfigurations(
                // TODO: Configure via user secrets or environment variables
                // See: https://docs.microsoft.com/aspnet/core/security/app-secrets
                ("Twilio:SendGrid:ApiKey", Environment.GetEnvironmentVariable("SENDGRID_API_KEY") ?? "[YOUR_SENDGRID_API_KEY]"),
                ("Twilio:SendGrid:Default:From:Email", "no-reply@oobdev.com"),
                ("Twilio:SendGrid:Default:From:Name", "OoBDev Dev"),
                ("Twilio:SendGrid:Default:Subject", "Hello from OoBDev"),

                ("Azure:Storage:Default:ConnectionString", "UseDevelopmentStorage=true")
            )
            .AddDebugTestServices(context)
            .AddSendGridServices()
            .AddToolkitServices()
            .AddAzureStorageServices()
            .AddComplexEventsServices()
            .AddCommunicationsServices()
            .AddMessageQueueingServices()
            ;

        public static T GetService<T>(IServiceCollection services = null, TestContext context = null) => (services ?? GetServices(context))
            .BuildServiceProvider()
            .GetRequiredService<T>()
            ;
    }
}