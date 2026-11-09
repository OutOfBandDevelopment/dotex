using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Tests;

[TestClass]
public class ChannelConfigurationTests
{
    private static ConfigurationChannelRepository Repository()
        => new(Options.Create(new SendReceiveOptions
        {
            Channels = [new ChannelDefinition { Name = "Mail", Protocol = "email", Provider = "mailkit" }],
        }));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetByNameAsync_IgnoresCase()
    {
        var channel = await Repository().GetByNameAsync("mail");

        Assert.IsNotNull(channel);
        Assert.AreEqual("mailkit", channel.Provider);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetByNameAsync_Unknown_ReturnsNull()
        => Assert.IsNull(await Repository().GetByNameAsync("sms"));

    private static Mock<IChannelProvider> Provider(string name, params string[] protocols)
    {
        var mock = new Mock<IChannelProvider>();
        mock.SetupGet(p => p.ProviderName).Returns(name);
        mock.SetupGet(p => p.SupportedProtocols).Returns(protocols);
        return mock;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetProvider_MatchesProviderAndProtocol()
    {
        var mail = Provider("mailkit", "email").Object;
        var registry = new ChannelRegistry([Provider("twilio", "sms").Object, mail]);

        Assert.AreSame(mail, registry.GetProvider("EMAIL", "MailKit"));
        Assert.IsNull(registry.GetProvider("sms", "mailkit"));
        Assert.IsNull(registry.GetProvider("email", "unknown"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task TryAddSendReceive_BindsChannelsFromConfiguration()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OoBDev:Communications:Channels:0:Name"] = "mail",
            ["OoBDev:Communications:Channels:0:Protocol"] = "email",
            ["OoBDev:Communications:Channels:0:Provider"] = "mailkit",
        }).Build();
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddSingleton(Provider("mailkit", "email").Object);

        services.TryAddSendReceive();
        using var provider = services.BuildServiceProvider();

        var channel = await provider.GetRequiredService<IChannelRepository>().GetByNameAsync("mail");
        Assert.AreEqual("email", channel?.Protocol);
        Assert.IsNotNull(provider.GetRequiredService<IChannelRegistry>().GetProvider("email", "mailkit"));
    }
}
