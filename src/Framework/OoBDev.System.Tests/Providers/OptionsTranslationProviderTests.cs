using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Providers;
using System.Collections.Generic;
using System.Globalization;

namespace OoBDev.System.Tests.Providers;

[TestClass]
public class OptionsTranslationProviderTests
{
    private static OptionsTranslationProvider Create() => new(Options.Create(new TranslationOptions
    {
        Cultures =
        {
            [""] = new Dictionary<string, string> { ["greeting"] = "Hello", ["only-neutral"] = "Neutral" },
            ["fr"] = new Dictionary<string, string> { ["greeting"] = "Bonjour" },
            ["fr-CA"] = new Dictionary<string, string> { ["greeting"] = "Allo" },
        },
    }));

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("fr-CA", "greeting", "Allo")]
    [DataRow("fr-FR", "greeting", "Bonjour")]
    [DataRow("de-DE", "greeting", "Hello")]
    [DataRow("fr-CA", "only-neutral", "Neutral")]
    [DataRow("fr-CA", "missing", null)]
    public void Translate_CultureChain_FallsBackToParents(string culture, string key, string? expected)
    {
        var result = Create().Translate(key, CultureInfo.GetCultureInfo(culture));

        Assert.AreEqual(expected, result);
    }
}
