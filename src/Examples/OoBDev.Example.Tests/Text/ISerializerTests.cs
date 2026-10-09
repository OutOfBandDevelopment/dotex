using OoBDev.Common;
using OoBDev.System.Text;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.System.Text.Xml.Serialization;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace OoBDev.Examples.Tests.Text;

[TestClass]
public class ISerializerTests
{
    public required TestContext TestContext { get; set; }

    private ServiceProvider CreateProvider(params KeyValuePair<string, string?>[] configs)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configs)
            .Build()
            ;
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .TryAllCommonExtensions(config,
                systemBuilder: new(),
                aspNetBuilder: new(),
                jwtBuilder: new(),
                identityBuilder: new(),
                externalBuilder: new(),
                hostingBuilder: new()
                )
            .BuildServiceProvider()
            ;

        return serviceProvider;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void DefaultSerializerTests()
    {
        var serviceProvider = CreateProvider();
        var service = serviceProvider.GetRequiredService<ISerializer>();
        TestContext.WriteLine($"{service.GetType()}");
        Assert.IsInstanceOfType<DefaultJsonSerializer>(service);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("json", typeof(DefaultJsonSerializer))]
    [DataRow("xml", typeof(DefaultXmlSerializer))]
    [DataRow("bson", typeof(DefaultBsonSerializer))]
    public void KeyedSerializerTests(string key, Type expected)
    {
        var serviceProvider = CreateProvider();
        var service = serviceProvider.GetRequiredKeyedService<ISerializer>(key);
        TestContext.WriteLine($"{service.GetType()}");
        Assert.IsInstanceOfType(service, expected);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("json", typeof(DefaultJsonSerializer), ".json")]
    [DataRow("xml", typeof(DefaultXmlSerializer), ".xml")]
    [DataRow("bson", typeof(DefaultBsonSerializer), ".bson")]
    public void KeyedSerializerTests_Value(string key, Type expected, string ext)
    {
        var serviceProvider = CreateProvider();
        var service = serviceProvider.GetRequiredKeyedService<ISerializer>(key);

        var result = service.Serialize(new TestObject { Prop1 = "Hello World" });

        TestContext.WriteLine($"{service.GetType()}");
        TestContext.AddResult(result, fileName: $"Result{ext}");
        Assert.IsInstanceOfType(service, expected);
    }

    /// <summary>
    /// example class
    /// </summary>
    public class TestObject
    {
        /// <summary>
        /// example property
        /// </summary>
        public required string Prop1 { get; set; }
    }
}
