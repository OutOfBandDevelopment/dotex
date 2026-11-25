using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Data.EntityFrameworkCore.Tests;

[TestClass]
public sealed class StoreInstanceTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("Unit")]
    public void Of_WithoutAnInstance_ReturnsTheDefault()
    {
        // Stage
        using var context = new NotesContext(new DbContextOptionsBuilder<NotesContext>().UseSqlite("Data Source=:memory:").Options);

        // Test
        var instance = StoreInstance.Of(context);
        TestContext.WriteLine($"schema={instance.Schema} prefix={instance.TablePrefix}");

        // Assert
        Assert.AreEqual(StoreInstanceOptions.Default, instance);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void UseStoreInstance_SchemaAndPrefix_PlaceTheTable()
    {
        // Stage
        var options = new DbContextOptionsBuilder<NotesContext>().UseSqlite("Data Source=:memory:").UseStoreInstance("a", "p_").Options;
        using var context = new NotesContext(options);

        // Test
        var entity = context.Model.FindEntityType(typeof(Note))!;
        TestContext.WriteLine($"schema={entity.GetSchema()} table={entity.GetTableName()}");

        // Assert
        Assert.AreEqual("a", entity.GetSchema());
        Assert.AreEqual("p_Notes", entity.GetTableName());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void UseStoreInstance_TwoInstancesOfOneContext_GetTheirOwnModel()
    {
        // Stage
        using var first = new NotesContext(new DbContextOptionsBuilder<NotesContext>().UseSqlite("Data Source=:memory:").UseStoreInstance("a").Options);
        using var second = new NotesContext(new DbContextOptionsBuilder<NotesContext>().UseSqlite("Data Source=:memory:").UseStoreInstance("b").Options);

        // Test
        var schemas = new[] { first, second }.Select(c => c.Model.FindEntityType(typeof(Note))!.GetSchema()).ToArray();
        TestContext.WriteLine(string.Join(",", schemas));

        // Assert
        CollectionAssert.AreEqual(new[] { "a", "b" }, schemas);
    }

    private sealed class Note
    {
        public int Id { get; set; }
    }

    private sealed class NotesContext(DbContextOptions<NotesContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder) =>
            StoreInstance.Of(this).ToTable(modelBuilder.Entity<Note>(), "Notes");
    }
}
