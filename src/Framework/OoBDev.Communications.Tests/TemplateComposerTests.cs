using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.Communications.Orchestration;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Tests;

[TestClass]
public class TemplateComposerTests
{
    public required TestContext TestContext { get; set; }

    private static readonly TargetPreference Preference = new("t1", ["email", "sms"], CultureInfo.GetCultureInfo("fr-FR"), TimeZoneInfo.Utc);

    private static CommunicationRequest Request(params (string Key, object? Value)[] data)
        => new("t1", "welcome", data.ToDictionary(d => d.Key, d => d.Value), CorrelationId: "c-1");

    private static Mock<ITemplateEngine> Engine(Dictionary<string, string> templates, Action<string>? onApply = null)
    {
        var engine = new Mock<ITemplateEngine>();
        engine.Setup(e => e.Get(It.IsAny<string>())).Returns((string name) => templates.ContainsKey(name) ? Mock.Of<ITemplateContext>() : null);
        engine.Setup(e => e.ApplyAsync(It.IsAny<string>(), It.IsAny<object>()))
            .ReturnsAsync((string name, object _) =>
            {
                onApply?.Invoke(CultureInfo.CurrentUICulture.Name);
                return templates[name];
            });
        return engine;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Email_WithTemplates_BuildsMessageInTargetCulture()
    {
        // Mock
        string? culture = null;
        var engine = Engine(new()
        {
            ["welcome.email.subject"] = " Hello \n",
            ["welcome.email.html"] = "<b>Hi</b>",
            ["welcome.email.text"] = "Hi",
        }, c => culture = c);
        var composer = new TemplateEmailComposer(engine.Object, Options.Create(new TemplateComposerOptions()));

        // Test
        var message = await composer.ComposeAsync(Request(("EmailAddress", "a@example.test")), Preference);
        TestContext.WriteLine($"culture during render: {culture}");

        // Assert
        var email = Assert.IsInstanceOfType<EmailMessage>(message);
        Assert.AreEqual("a@example.test", email.To.Single());
        Assert.AreEqual("Hello", email.Subject);
        Assert.AreEqual("<b>Hi</b>", email.HtmlContent);
        Assert.AreEqual("Hi", email.TextContent);
        Assert.AreEqual("c-1", email.CorrelationId);
        Assert.AreEqual("fr-FR", culture);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Email_NoAddressOrNoBody_ReturnsNull()
    {
        // Mock
        var engine = Engine(new() { ["welcome.email.subject"] = "Hello" });
        var composer = new TemplateEmailComposer(engine.Object, Options.Create(new TemplateComposerOptions()));

        // Test
        var noAddress = await composer.ComposeAsync(Request(), Preference);
        var noBody = await composer.ComposeAsync(Request(("EmailAddress", "a@example.test")), Preference);

        // Assert
        Assert.IsNull(noAddress);
        Assert.IsNull(noBody);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Sms_WithTemplate_BuildsMessage()
    {
        // Mock
        var engine = Engine(new() { ["welcome.sms"] = "Hi there\n" });
        var composer = new TemplateSmsComposer(engine.Object, Options.Create(new TemplateComposerOptions()));

        // Test
        var message = await composer.ComposeAsync(Request(("PhoneNumber", "+15551230000")), Preference);

        // Assert
        var sms = Assert.IsInstanceOfType<SmsMessage>(message);
        Assert.AreEqual("+15551230000", sms.To);
        Assert.AreEqual("Hi there", sms.Body);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Sms_MissingNumberOrTemplate_ReturnsNull()
    {
        // Mock
        var composer = new TemplateSmsComposer(Engine([]).Object, Options.Create(new TemplateComposerOptions()));

        // Test
        var noNumber = await composer.ComposeAsync(Request(), Preference);
        var noTemplate = await composer.ComposeAsync(Request(("PhoneNumber", "+15551230000")), Preference);

        // Assert
        Assert.IsNull(noNumber);
        Assert.IsNull(noTemplate);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddTemplateComposers_RegistersBothAndHonoursOptions()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<ITemplateEngine>());

        // Test
        services.TryAddTemplateComposers(o => o.SmsChannelName = "text");
        services.TryAddTemplateComposers();
        using var provider = services.BuildServiceProvider();
        var channels = provider.GetServices<IMessageComposer>().Select(c => c.ChannelName).Order().ToArray();
        TestContext.WriteLine(string.Join(",", channels));

        // Assert
        CollectionAssert.AreEqual(new[] { "email", "text" }, channels);
    }
}
