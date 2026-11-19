using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Handlebars.Templating;
using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating.EntityFrameworkCore;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Handlebars.Tests.Templating;

[TestClass]
public class HandlebarsDatabaseSourceTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task DatabaseTemplate_WithHandlebarsContentType_RendersThroughTheProvider()
    {
        var connectionString = $"Data Source=hbs_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        await using var keeper = new SqliteConnection(connectionString);
        keeper.Open();
        var factory = new Factory(connectionString);
        using (var db = factory.CreateDbContext())
        {
            db.Database.EnsureCreated();
        }

        var repository = new EfTemplateRepository(factory, TimeProvider.System);
        await repository.PublishAsync(new TemplatePublish("greeting", ContentTypesExtensions.Text.HandlebarsTemplate, "Hello {{name}}!", "tester"));
        var source = new EfTemplateSource(repository, Options.Create(new EfTemplateSourceOptions()));
        var context = source.Get("greeting").Single();

        var provider = new HandlebarsTemplateProvider([], [], [], TestLogger.CreateLogger<HandlebarsTemplateProvider>());
        using var output = new MemoryStream();
        var applied = await provider.ApplyAsync(context, new { name = "Ann" }, output);

        output.Position = 0;
        var text = new StreamReader(output).ReadToEnd();
        TestContext.WriteLine(text);
        Assert.IsTrue(applied);
        Assert.AreEqual("Hello Ann!", text);
    }

    private sealed class Factory(string connectionString) : IDbContextFactory<TemplatesDbContext>
    {
        public TemplatesDbContext CreateDbContext() =>
            new(new DbContextOptionsBuilder<TemplatesDbContext>().UseSqlite(connectionString).Options);
    }
}
