using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class BlePlatformAdapterLoaderTests
{
    [TestMethod]
    public void TryRegisterPlatformAdapter_BackendAssemblyAbsent_LeavesTheRegistrationsUntouched()
    {
        // Stage
        Assert.IsFalse(File.Exists(Path.Combine(AppContext.BaseDirectory, "OoBDev.System.IO.Bluetooth.Windows.dll")), "The test host must not carry the backend.");
        var services = new ServiceCollection();

        // Test
        BlePlatformAdapterLoader.TryRegisterPlatformAdapter(services);

        // Assert
        Assert.AreEqual(0, services.Count);
    }

}
