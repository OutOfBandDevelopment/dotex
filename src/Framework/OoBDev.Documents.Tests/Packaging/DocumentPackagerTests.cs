using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Packaging;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Tests.Packaging;

[TestClass]
public class DocumentPackagerTests
{
    public required TestContext TestContext { get; set; }

    private static IDocumentPackager Packager()
    {
        var services = new ServiceCollection();
        services.TryAddDocumentServices();
        return services.BuildServiceProvider().GetRequiredService<IDocumentPackager>();
    }

    private static PackageItem Item(string name, string text) => new(name, Encoding.UTF8.GetBytes(text));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PackAsync_Zip_ListsEachEntryWithItsContent()
    {
        // Stage
        PackageItem[] items = [Item("a.txt", "alpha"), Item("docs/b.txt", "beta")];
        using var stream = new MemoryStream();

        // Test
        await Packager().PackAsync("ZIP", items, stream);
        TestContext.WriteLine("Package bytes: {0}", stream.Length);

        // Assert
        stream.Position = 0;
        using var zip = new ZipArchive(stream);
        CollectionAssert.AreEqual(new[] { "a.txt", "docs/b.txt" }, zip.Entries.Select(e => e.FullName).ToArray());
        using var reader = new StreamReader(zip.GetEntry("docs/b.txt")!.Open());
        Assert.AreEqual("beta", await reader.ReadToEndAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PackAsync_UnknownType_Throws()
    {
        using var stream = new MemoryStream();
        await Assert.ThrowsExactlyAsync<PackagingException>(() => Packager().PackAsync("rar", [Item("a", "x")], stream));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("../evil.txt")]
    [DataRow("/rooted.txt")]
    [DataRow("a\\b.txt")]
    [DataRow("C:x.txt")]
    [DataRow("")]
    public async Task PackAsync_UnsafeName_Throws(string name)
    {
        using var stream = new MemoryStream();
        await Assert.ThrowsExactlyAsync<PackagingException>(() => Packager().PackAsync("zip", [Item(name, "x")], stream));
        Assert.AreEqual(0, stream.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PackAsync_DuplicateName_Throws()
    {
        using var stream = new MemoryStream();
        List<PackageItem> items = [Item("a.txt", "1"), Item("A.txt", "2")];
        await Assert.ThrowsExactlyAsync<PackagingException>(() => Packager().PackAsync("zip", items, stream));
    }
}
