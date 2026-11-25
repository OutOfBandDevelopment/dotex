using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Threading.Tasks;

namespace OoBDev.Communications.Unsubscribe.Tests;

[TestClass]
public class UnsubscribeLinksTests
{
    private const string Key = "0123456789abcdef0123456789abcdef";

    private static (UnsubscribeLinks Links, InMemorySuppressionList List) Create(string key = Key, string url = "https://example.test/unsubscribe")
    {
        var list = new InMemorySuppressionList();
        return (new UnsubscribeLinks(Options.Create(new UnsubscribeOptions { EndpointUrl = url, SigningKey = key }), list), list);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Token_RoundTrips_AndNormalisesCase()
    {
        var (links, _) = Create();

        var ok = links.TryReadToken(links.CreateToken("Ann@Example.TEST"), out var address);

        Assert.IsTrue(ok);
        Assert.AreEqual("ann@example.test", address);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryReadToken_TamperedOrForeignOrMalformed_IsRejected()
    {
        var (links, _) = Create();
        var (other, _) = Create("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");
        var token = links.CreateToken("ann@example.test");
        var tampered = links.CreateToken("bob@example.test").Split('.')[0] + "." + token.Split('.')[1];

        Assert.IsFalse(links.TryReadToken(tampered, out _));
        Assert.IsFalse(other.TryReadToken(token, out _));
        Assert.IsFalse(links.TryReadToken(null, out _));
        Assert.IsFalse(links.TryReadToken("nodot", out _));
        Assert.IsFalse(links.TryReadToken("a.b.c", out _));
        Assert.IsFalse(links.TryReadToken("!!!.???", out _));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateHeaders_GivesOneClickHeaders()
    {
        var (links, _) = Create();

        var headers = links.CreateHeaders("ann@example.test");

        StringAssert.StartsWith(headers["List-Unsubscribe"], "<https://example.test/unsubscribe?t=");
        StringAssert.EndsWith(headers["List-Unsubscribe"], ">");
        Assert.AreEqual("List-Unsubscribe=One-Click", headers["List-Unsubscribe-Post"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CreateHeaders_UrlWithQuery_UsesAmpersand()
    {
        var (links, _) = Create(url: "https://example.test/u?list=news");

        StringAssert.Contains(links.CreateHeaders("a@b.test")["List-Unsubscribe"], "?list=news&t=");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task UnsubscribeAsync_ValidToken_Suppresses_InvalidDoesNot()
    {
        var (links, list) = Create();

        var done = await links.UnsubscribeAsync(links.CreateToken("ann@example.test"));
        var bad = await links.UnsubscribeAsync("bad.token");

        Assert.AreEqual("ann@example.test", done);
        Assert.IsNull(bad);
        Assert.IsTrue(await list.IsSuppressedAsync("ann@example.test"));
    }
}
