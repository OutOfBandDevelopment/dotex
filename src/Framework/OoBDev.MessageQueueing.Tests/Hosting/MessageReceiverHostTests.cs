using OoBDev.MessageQueueing.Hosting;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.Hosting;

[TestClass]
public class MessageReceiverHostTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StartStopTest()
    {
        var cancellationTokenSourceBlocker = new CancellationTokenSource();
        var cancellationTokenSource = new CancellationTokenSource();

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockReceiverProviderFactory = mockRepo.Create<IMessageReceiverProviderFactory>();
        var mockReceiverProvider = mockRepo.Create<IMessageReceiverProvider>();

        mockReceiverProvider
            .Setup(s => s.RunAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(async () =>
            {
                TestContext.WriteLine("Ran Child!");
                await cancellationTokenSourceBlocker.CancelAsync();
                await Task.Delay(100);
            })
            ;
        mockReceiverProviderFactory
            .Setup(s => s.Create())
            .Returns([mockReceiverProvider.Object])
            ;

        using (var host = new MessageReceiverHost(
             TestLogger.CreateLogger<MessageReceiverHost>(),
             mockReceiverProviderFactory.Object,
             Options.Create(new MessageReceiverHostOptions { RestartDelay = TimeSpan.FromMilliseconds(10) })
             ))
        {
            await host.StartAsync(cancellationTokenSource.Token);

            var spinCount = 0;
            while (!cancellationTokenSourceBlocker.IsCancellationRequested)
            {
                TestContext.WriteLine($"Spin! : {spinCount++}");
                await Task.Yield();
            }
            await host.StopAsync(cancellationTokenSource.Token);
        }

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StartAsync_ReceiverFails_RestartsAfterTheConfiguredDelay()
    {
        // Stage
        using var stop = new CancellationTokenSource();
        var runs = 0;
        var provider = new Mock<IMessageReceiverProvider>();
        provider.Setup(p => p.RunAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                if (Interlocked.Increment(ref runs) >= 3)
                {
                    stop.Cancel();
                }

                return Task.FromException(new InvalidOperationException("boom"));
            });
        var factory = new Mock<IMessageReceiverProviderFactory>();
        factory.Setup(f => f.Create()).Returns([provider.Object]);
        using var host = new MessageReceiverHost(
            TestLogger.CreateLogger<MessageReceiverHost>(),
            factory.Object,
            Options.Create(new MessageReceiverHostOptions { RestartDelay = TimeSpan.FromMilliseconds(10) }));

        // Test
        await host.StartAsync(stop.Token);
        await Task.Delay(500, TestContext.CancellationToken);

        // Assert
        Assert.IsGreaterThanOrEqualTo(3, runs);
    }}
