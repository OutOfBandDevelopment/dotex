using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.WorkQueue.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class ConfigurationBindingTests
{
    [TestMethod]
    public void TryAddWorkQueue_BindsStageOverrides_AndEnabledByName()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["WorkQueue:BatchSize"] = "8",
            ["WorkQueue:Default:MaxAttempts"] = "5",
            ["WorkQueue:Stages:hash:Enabled"] = "false",
            ["WorkQueue:Stages:ocr:Lease"] = "01:00:00",
        }).Build();
        var services = new ServiceCollection();
        services.TryAddWorkQueue(configuration);
        using var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<WorkQueueOptions>>().Value;

        Assert.AreEqual(8, options.BatchSize);
        Assert.AreEqual(5, options.Default.MaxAttempts);
        Assert.IsFalse(options.For("hash").Enabled);
        Assert.AreEqual(TimeSpan.FromHours(1), options.For("ocr").Lease);
        Assert.IsNotNull(provider.GetService<TimeProvider>());
    }

    [TestMethod]
    public void TryAddWorkQueue_RejectsABatchSizeOutOfRange()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["WorkQueue:BatchSize"] = "0",
        }).Build();
        var services = new ServiceCollection();
        services.TryAddWorkQueue(configuration);
        using var provider = services.BuildServiceProvider();

        Assert.ThrowsExactly<OptionsValidationException>(() => provider.GetRequiredService<IOptions<WorkQueueOptions>>().Value.ToString());
    }
}
