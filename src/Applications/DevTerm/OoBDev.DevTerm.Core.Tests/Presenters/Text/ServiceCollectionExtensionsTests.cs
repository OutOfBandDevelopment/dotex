using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Presenters.Text.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void TryAddTextPresenters_Twice_RegistersEachPresenterOnce()
    {
        // Stage
        var services = new ServiceCollection().TryAddTextPresenters().TryAddTextPresenters();

        // Test
        using var provider = services.BuildServiceProvider();
        var names = provider.GetServices<IPresenter>().Select(p => p.Name).Order().ToArray();

        // Assert
        CollectionAssert.AreEqual(new[] { "ascii", "binary", "decimal", "hex", "keyvalue", "octal", "utf8" }, names);
    }
}
