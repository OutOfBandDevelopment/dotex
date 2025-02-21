using OoBDev.Extensions.Configuration;
using OoBDev.Identity;
using OoBDev.Microsoft.B2C.Identity;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.B2C.Tests;

[TestClass]
public class ManageGraphUserIntegrationTests
{
    public required TestContext TestContext { get; set; }

    private static IConfiguration GetConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(
                        ("MicrosoftIdentityOptions:ClientID", "6721294c-f956-4290-9629-6455b92fbcf2"),
                        ("MicrosoftIdentityOptions:Issuer", "f94cddd5-af89-42f6-9a81-d7898aef64a3"),
                        ("MicrosoftIdentityOptions:Tenant", "oobdevexampledev"),
                        ("MicrosoftIdentityOptions:ClientSecret", "bf68Q~SI7x0osvcUaA8qR-oE9xA3ZTqNZVvl.b86")
                )
            .Build();

    private static ServiceProvider GetIntegrationServiceProvider()
    {
        var services = new ServiceCollection()
            .AddLogging(logging => logging.AddConsole())
            .AddSingleton<IIdentityManager, ManageGraphUser>()
            ;

        services.Replace(ServiceDescriptor.Transient(_ => GetConfiguration()));

        return services.BuildServiceProvider();
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task GetGraphUsersByEmailTest_Integration_Exists()
    {
        // Stage
        var serviceProvider = GetIntegrationServiceProvider();
        var emailAddress = $"mwwhited@gmail.com";

        // Mock

        // Test
        var manageGraphUser = serviceProvider.GetRequiredService<IIdentityManager>();

        var result = await manageGraphUser.GetIdentityUsersByEmail(emailAddress)
            ?? throw new ApplicationException($"{nameof(manageGraphUser.GetIdentityUsersByEmail)} should return a value");

        foreach (var item in result)
        {
            TestContext.WriteLine($"item: {item.FirstName} {item.LastName} ({item.UserName})");
        }

        TestContext.WriteLine($"emailAddress: {emailAddress}");
        TestContext.WriteLine($"result: {result}");

        // Assert
        foreach (var customer in result.ToArray())
            Assert.AreNotEqual(Guid.Empty.ToString(), customer.EmailAddress);

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task CreateIdentityUserAsyncTest_Integration_Exists()
    {
        // Stage
        var serviceProvider = GetIntegrationServiceProvider();
        var emailAddress = $"mwwhited@gmail.com";

        // Mock

        // Test
        var manageGraphUser = serviceProvider.GetRequiredService<IIdentityManager>();

        var result = await manageGraphUser.CreateIdentityUserAsync(
            email: emailAddress,
            firstName: "Matthew",
        lastName: "Whited");

        TestContext.WriteLine($"emailAddress: {emailAddress}");
        TestContext.WriteLine($"result: {result}");

        // Assert
        Assert.AreNotEqual(Guid.Empty.ToString(), result.objectId);

        // Verify
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task RemoveIdentityUserAsyncTest_Integration()
    {
        // Stage
        var serviceProvider = GetIntegrationServiceProvider();

        // Mock

        // Test
        var manageGraphUser = serviceProvider.GetRequiredService<IIdentityManager>();
        var userId = "581c2f51-bda2-4ca2-9bab-6fedd1470fc1";

        var result = await manageGraphUser.RemoveIdentityUserAsync(userId);

        // Assert
        Assert.IsTrue(result);

        // Verify
    }
}
