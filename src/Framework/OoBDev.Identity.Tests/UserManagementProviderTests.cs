using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Identity.Tests;

[TestClass]
public class UserManagementProviderTests
{
    private sealed class FakeIdentityManager(string? password) : IIdentityManager
    {
        public string? Email { get; private set; }

        public Task<(string objectId, string? password)> CreateIdentityUserAsync(string email, string firstName, string lastName)
        {
            Email = $"{email}|{firstName}|{lastName}";
            return Task.FromResult(("object-1", password));
        }

        public Task<IList<UserIdentityModel>?> GetIdentityUsersByEmail(string emailAddress) =>
            Task.FromResult<IList<UserIdentityModel>?>(null);

        public Task<bool> RemoveIdentityUserAsync(string userId) => Task.FromResult(true);
    }

    private static readonly UserCreateModel _model = new()
    {
        FirstName = "Ada",
        LastName = "Lovelace",
        EmailAddress = "ada@example.org",
    };

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CreateAccountAsync_WithManager_MapsIdAndPassword()
    {
        // Stage
        var manager = new FakeIdentityManager("secret");
        var provider = new UserManagementProvider(manager);

        // Test
        var created = await provider.CreateAccountAsync(_model);

        // Assert
        Assert.AreEqual("object-1", created.Username);
        Assert.AreEqual("secret", created.Password);
        Assert.AreEqual("ada@example.org|Ada|Lovelace", manager.Email);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CreateAccountAsync_WithoutPassword_ReturnsEmptyPassword()
    {
        // Stage
        var provider = new UserManagementProvider(new FakeIdentityManager(null));

        // Test
        var created = await provider.CreateAccountAsync(_model);

        // Assert
        Assert.AreEqual("", created.Password);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task CreateAccountAsync_WithoutManager_ThrowsNotSupported()
    {
        // Stage
        var provider = new UserManagementProvider();

        // Test and Assert
        _ = await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.CreateAccountAsync(_model));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryAddIdentityServices_Twice_RegistersTheProviderOnce()
    {
        // Stage
        var services = new ServiceCollection();

        // Test
        _ = services.TryAddIdentityServices(null!);
        _ = services.TryAddIdentityServices(null!);

        // Assert
        Assert.AreEqual(1, services.Count(d => d.ServiceType == typeof(IUserManagementProvider)));
    }
}
