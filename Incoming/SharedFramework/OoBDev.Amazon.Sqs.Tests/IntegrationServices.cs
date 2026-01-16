using OoBDev.ComplexEvents.Common;
using OoBDev.DocumentCenter;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Amazon.Sqs.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices(this TestContext testContext) => new ServiceCollection()
            .AddDebugTestConfigurations(
                // TODO: Configure via user secrets or environment variables
                // See: https://docs.microsoft.com/aspnet/core/security/app-secrets
                // Format: Region=us-east-2;AccessKeyId=[YOUR_AWS_ACCESS_KEY];SecretAccessKey=[YOUR_AWS_SECRET_KEY]
                ("Amazon:SimpleQueue:Default:ConnectionString",
                    Environment.GetEnvironmentVariable("AWS_SQS_CONNECTION_STRING") ??
                    "Region=us-east-2;AccessKeyId=[YOUR_AWS_ACCESS_KEY_ID];SecretAccessKey=[YOUR_AWS_SECRET_ACCESS_KEY]")
            )
            .AddAmazonSqsServices()
            .AddTestLoggingServices(testContext)
            .AddToolkitServices()
            .AddComplexEventsServices()
            .AddDocumentCenterServices()
            .AddMessageQueueingServices()
            ;

        public static T GetService<T>(this TestContext testContext, IServiceCollection? services = null) => (services ?? testContext.GetServices())
            .BuildServiceProvider()
            .GetService<T>()
            ;
    }
}