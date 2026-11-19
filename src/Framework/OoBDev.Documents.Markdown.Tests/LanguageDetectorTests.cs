using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Markdown.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class LanguageDetectorTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [DataRow("The quick report was sent to the office on Monday and it is clear that the results are not what they hoped for, but the team will review them with the board.", "en")]
    [DataRow("Der Bericht wurde am Montag an das Büro geschickt und es ist klar, dass die Ergebnisse nicht so sind, wie sie es sich gewünscht haben, aber das Team wird sie mit dem Vorstand prüfen.", "de")]
    [DataRow("Le rapport a été envoyé au bureau lundi et il est clair que les résultats ne sont pas ceux qu'ils espéraient, mais l'équipe les examinera avec le conseil pour la suite des travaux.", "fr")]
    [DataRow("El informe fue enviado a la oficina el lunes y es claro que los resultados no son lo que se esperaba, pero el equipo los revisará con la junta para decidir lo que se hará.", "es")]
    public void Detect_ProseInAKnownLanguage_ReturnsItsCode(string text, string expected)
    {
        var language = LanguageDetector.Detect(text);

        TestContext.WriteLine("Detected {0} (expected {1})", language, expected);
        Assert.AreEqual(expected, language);
    }

    [TestMethod]
    public void Detect_ShortOrEmptyOrNoStopWords_ReturnsNull()
    {
        Assert.IsNull(LanguageDetector.Detect(null));
        Assert.IsNull(LanguageDetector.Detect("   "));
        Assert.IsNull(LanguageDetector.Detect("the cat sat"));
        Assert.IsNull(LanguageDetector.Detect("1234 5678 9012 3456 7890 1234 5678 9012 3456 7890 1234 5678 9012 3456 7890 1234 5678 9012 3456 7890 1234"));
    }
}
