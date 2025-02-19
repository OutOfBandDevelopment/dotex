using OoBDev.Extensions;
using OoBDev.MessageQueueing.Services;
using OoBDev.MessageQueueing.Tests.TestItems;
using OoBDev.System;
using OoBDev.System.Accessors;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests;

[TestClass]
public class MessageSenderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SendAsyncTest_ByFullType()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"MessageQueue:MessageSenderTests:Provider", typeof(TestMessageSenderProvider).AssemblyQualifiedName },
        });

        var config = configBuilder.Build();

        var service = GetServiceProvider(TestContext, config);

        // ---------------

        var sender = service.GetRequiredService<IMessageQueueSender<MessageSenderTests>>();
        var correlationId = await sender.SendAsync(new
        {
            hello = "world",
        });

        TestContext.Write($"correlationId: {correlationId}");
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    [ExpectedException(typeof(ApplicationException))]
    public async Task SendAsyncTest_Error()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"MessageQueue:MessageSenderTests:Provider", typeof(TestExceptionMessageSenderProvider).AssemblyQualifiedName },
        });

        var config = configBuilder.Build();

        var service = GetServiceProvider(TestContext, config);

        // ---------------

        var sender = service.GetRequiredService<IMessageQueueSender<MessageSenderTests>>();
        var correlationId = await sender.SendAsync(new
        {
            hello = "world",
        });

        TestContext.Write($"correlationId: {correlationId}");

        Assert.Fail("you should not get here!");
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task SendAsyncTest_ByKeyed()
    {
        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {"MessageQueue:MessageSenderTests:Provider", TestMessageSenderProvider.ProviderName },
        });

        var config = configBuilder.Build();

        var service = GetServiceProvider(TestContext, config, services => services.AddKeyedTransient<IMessageSenderProvider, TestMessageSenderProvider>(TestMessageSenderProvider.ProviderName));

        // ---------------

        var sender = service.GetRequiredService<IMessageQueueSender<MessageSenderTests>>();
        var correlationId = await sender.SendAsync(new
        {
            hello = "world",
        });

        TestContext.Write($"correlationId: {correlationId}");
    }

    public static IServiceProvider GetServiceProvider(TestContext testContext, IConfiguration config, Action<IServiceCollection>? update = default)
    {
        testContext.AddResult(config);

        var serviceCollection = new ServiceCollection()
            .TryAddMessageQueueingServices()
            .TryAddSystemExtensions(config, new())
            ;

        serviceCollection.AddLogging(builder => builder
            .AddConsole()
            .SetMinimumLevel(LogLevel.Debug)
            );
        serviceCollection.AddTransient(_ => config);
        serviceCollection.AddTransient(_ => testContext);

        serviceCollection.AddAccessor<ClaimsPrincipal>();
        serviceCollection.AddTransient(sp =>
        {
            var principalAccessor = sp.GetRequiredService<IAccessor<ClaimsPrincipal>>();

            var principal = principalAccessor.Value ??= new ClaimsPrincipal();
            principal.AddIdentity(new ClaimsIdentity(new GenericIdentity("test-user")));

            return principal;
        });

        update?.Invoke(serviceCollection);

        var service = serviceCollection.BuildServiceProvider();
        return service;
    }
}
