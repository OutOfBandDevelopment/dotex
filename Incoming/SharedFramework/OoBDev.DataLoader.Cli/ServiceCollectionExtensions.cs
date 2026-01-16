using OoBDev.Azure.Storage;
using OoBDev.ComplexEvents.Common;
using OoBDev.DocumentCenter;
using OoBDev.IdentityModel.Extensions;
using OoBDev.MessageQueueing;
using OoBDev.Toolkit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace OoBDev.DataLoader.Cli
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRequiredFramework(this IServiceCollection services) => services
            .AddLogging(opt =>
            {
                opt.AddConsole();
                //#if DEBUG
                //                opt.SetMinimumLevel(LogLevel.Debug);
                //#endif
            })
            .AddSingleton<IConfiguration>(_ => new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddCommandLine(Environment.GetCommandLineArgs(), DataloaderConfig.CommandLineSwitchMappings)
                .Build()
            )
            .AddDocumentCenterServices()
            .AddAzureStorageServices()
            .AddComplexEventsServices()
            .AddMessageQueueingServices()
            .AddToolkitServices()
            .AddIdentityModelExtensions()
            ;
    }
}
