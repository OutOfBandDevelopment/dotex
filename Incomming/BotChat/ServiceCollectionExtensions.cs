using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;
using BotChat.Clients;
using BotChat.HostRunner;
using BotChat.KernelHost;
using BotChat.Ollama;

namespace BotChat;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKernelPlugIns(this IServiceCollection services) =>
        services
            .AddKernelPlugIn<BotChatPlugIn>()
        ;

    public static IServiceCollection AddKernelPlugIn<T>(this IServiceCollection services) where T : class, IKernelPlugIn
    {
        services.AddTransient<IKernelPlugIn, T>();
        return services;
    }

    public static IServiceCollection AddOllamaServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<OllamaOptions>()
            .Bind(configuration.GetSection(OllamaOptions.OptionName))
            .ValidateOnStart();

        services.TryAddTransient<IOllamaApiClientFactory, OllamaApiClientFactory>();
        services.TryAddTransient(sp => sp.GetRequiredService<IOllamaApiClientFactory>().Create());

        services.TryAddTransient<IOllamaServiceClientFactory, OllamaServiceClientFactory>();
        services.TryAddTransient(sp => sp.GetRequiredService<IOllamaServiceClientFactory>().GetTextGenerationService());

         services.AddSingleton<IChatCompletionService>(serviceProvider =>
        {
            var ollamaClient = serviceProvider.GetService<OllamaApiClient>();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            var builder = ((IChatClient)ollamaClient)
                .AsBuilder()
                .UseFunctionInvocation(loggerFactory, config => config.MaximumIterationsPerRequest = 128);

            if (loggerFactory is not null)
            {
                builder.UseLogging(loggerFactory);
            }

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            return builder.Build(serviceProvider).AsChatCompletionService(serviceProvider);
#pragma warning restore SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        });

        return services;
    }

    public static IServiceCollection AddKernelHostExtensions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddKernelPlugIns()
            .AddOllamaServices(configuration)
            .AddBotChatClient(configuration)
            .AddHostedService<RunnerHost<KernelRunner>>()
            ;

        services.TryAddKeyedTransient(KernelRunner.KernelName, (sp, key) =>
        {
            var registeredPlugins = sp.GetServices<IKernelPlugIn>();

            var plugins = new KernelPluginCollection();
            foreach (var plugin in registeredPlugins)
            {
                plugins.AddFromObject(plugin);
            }

            var kernel = new Kernel(sp, plugins);

            return kernel;
        });

        return services;
    }

    public static IServiceCollection AddBotChatClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<BotChatOptions>()
            .Bind(configuration.GetSection(BotChatOptions.OptionName))
            .ValidateOnStart();

        services.TryAddTransient<IBotChatClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<BotChatOptions>>().Value;
            var client = ActivatorUtilities.CreateInstance<BotChatClient>(sp, options.BaseUrl);
            return client;
        });
        return services;
    }
}

