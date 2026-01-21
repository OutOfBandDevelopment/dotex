using OoBDev.MongoDB.Extensions;
using OoBDev.System;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.MongoDB.Tests;

[TestClass]
public class MongoDBIntegrationTests
{
    public required TestContext TestContext { get; set; }

    private string? _databaseName;
    private IMongoClient? _mongoClient;

    [TestInitialize]
    public void TestInitialize()
    {
        // Create unique database name for this test run
        _databaseName = $"IntegrationTest_{Guid.NewGuid():N}";
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        // Cleanup: Drop the test database
        if (_mongoClient != null && _databaseName != null)
        {
            await _mongoClient.DropDatabaseAsync(_databaseName);
            TestContext.WriteLine($"Cleaned up database: {_databaseName}");
        }
    }

    private ITestMongoDatabase BuildTestDatabase()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDatabase:DatabaseName", _databaseName },
                { "MongoDatabase:ConnectionString", connectionString },
            })
            .Build();

        var services = new ServiceCollection();
        services.TryAddMongoServices(config, "MongoDatabase");
        services.TryAddSystemExtensions(config, new());
        services.TryAddMongoDatabase<ITestMongoDatabase>();

        var provider = services.BuildServiceProvider();

        // Get the database factory to access the underlying IMongoDatabase and its Client
        var factory = provider.GetRequiredService<IMongoDatabaseFactory>();
        var mongoDatabase = factory.Create<MongoDatabaseOptions>();
        _mongoClient = mongoDatabase.Client;

        return provider.GetRequiredService<ITestMongoDatabase>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task InsertDocument_WithNewEntity_AssignsIdAndPersists()
    {
        // Arrange
        var db = BuildTestDatabase();
        var entity = new TestCollection
        {
            Value1 = Guid.NewGuid().ToString(),
            Date = DateTimeOffset.Now,
            Value2 = "Test Value",
        };

        Assert.IsNull(entity.TestId, "TestId should be null before insert");

        // Act
        await db.Tests.InsertOneAsync(entity);

        // Assert
        Assert.IsNotNull(entity.TestId, "TestId should be assigned after insert");

        // Verify document was persisted
        var retrieved = await db.Tests.AsQueryable()
            .FirstOrDefaultAsync(e => e.TestId == entity.TestId);

        Assert.IsNotNull(retrieved, "Document should be retrievable after insert");
        Assert.AreEqual(entity.Value1, retrieved.Value1, "Value1 should match");
        Assert.AreEqual(entity.Value2, retrieved.Value2, "Value2 should match");

        TestContext.WriteLine($"Created document with TestId: {entity.TestId}");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task UpsertDocument_WhenDocumentExists_UpdatesExistingDocument()
    {
        // Arrange
        var db = BuildTestDatabase();
        var originalValue = "Original Value";
        var updatedValue = "Updated via Upsert";

        var entity = new TestCollection
        {
            Value1 = originalValue,
            Date = DateTimeOffset.Now,
            Value2 = "Test",
        };

        // First insert the document
        await db.Tests.InsertOneAsync(entity);
        Assert.IsNotNull(entity.TestId, "TestId should be assigned after insert");

        // Modify the entity for upsert
        entity.Value1 = updatedValue;

        var filter = Builders<TestCollection>.Filter.Eq(e => e.TestId, entity.TestId);
        var replaceOptions = new FindOneAndReplaceOptions<TestCollection>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After,
        };

        // Act
        var result = db.Tests.FindOneAndReplace(filter, entity, replaceOptions);

        // Assert
        Assert.IsNotNull(result, "Upsert should return the updated document");
        Assert.AreEqual(entity.TestId, result.TestId, "TestId should remain the same");
        Assert.AreEqual(updatedValue, result.Value1, "Value1 should be updated");

        // Verify only one document exists
        var count = await db.Tests.AsQueryable().CountAsync();
        Assert.AreEqual(1, count, "Should have exactly one document (updated, not duplicated)");

        TestContext.WriteLine($"Upserted document with TestId: {result.TestId}, Value1: '{originalValue}' -> '{updatedValue}'");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task InsertMultipleDocuments_WithVariedCasing_PersistsAllDocuments()
    {
        // Arrange
        var db = BuildTestDatabase();
        var insertedCount = 10;

        // Act - Insert documents with varied casing for Value1
        for (var i = 0; i < insertedCount; i++)
        {
            var entity = new TestCollection
            {
                Value1 = (i % 3) switch
                {
                    0 => "UPPER",
                    1 => "Upper",
                    2 => "upper",
                    _ => throw new NotSupportedException(),
                },
                Value2 = $"Document {i} - Variant {i % 3}",
                Date = DateTimeOffset.Now,
            };
            await db.Tests.InsertOneAsync(entity);
        }

        // Assert - Verify all documents were inserted
        var totalCount = await db.Tests.AsQueryable().CountAsync();
        Assert.AreEqual(insertedCount, totalCount, $"Should have inserted {insertedCount} documents");

        // Verify distribution of case variants
        var upperCount = await db.Tests.AsQueryable().Where(e => e.Value1 == "UPPER").CountAsync();
        var mixedCount = await db.Tests.AsQueryable().Where(e => e.Value1 == "Upper").CountAsync();
        var lowerCount = await db.Tests.AsQueryable().Where(e => e.Value1 == "upper").CountAsync();

        TestContext.WriteLine($"Inserted documents - UPPER: {upperCount}, Upper: {mixedCount}, upper: {lowerCount}");

        Assert.AreEqual(4, upperCount, "Should have 4 'UPPER' documents (indices 0, 3, 6, 9)");
        Assert.AreEqual(3, mixedCount, "Should have 3 'Upper' documents (indices 1, 4, 7)");
        Assert.AreEqual(3, lowerCount, "Should have 3 'upper' documents (indices 2, 5, 8)");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task QueryDocuments_WithOrdering_ReturnsSortedResults()
    {
        // Arrange - Insert test data first (make test independent)
        var db = BuildTestDatabase();

        var testData = new[]
        {
            new TestCollection { Value1 = "Charlie", Value2 = "Third", Date = DateTimeOffset.Now },
            new TestCollection { Value1 = "Alpha", Value2 = "First", Date = DateTimeOffset.Now },
            new TestCollection { Value1 = "Bravo", Value2 = "Second", Date = DateTimeOffset.Now },
            new TestCollection { Value1 = "Alpha", Value2 = "Another First", Date = DateTimeOffset.Now },
        };

        foreach (var entity in testData)
        {
            await db.Tests.InsertOneAsync(entity);
        }

        // Act - Query with ordering
        var orderedResults = await db.Tests.AsQueryable()
            .OrderBy(e => e.Value1)
            .ThenBy(e => e.Value2)
            .Select(e => new { e.Value1, e.Value2 })
            .ToListAsync();

        // Assert
        Assert.HasCount(4, orderedResults, "Should return all 4 documents");

        // Verify ordering: Alpha (Another First), Alpha (First), Bravo, Charlie
        Assert.AreEqual("Alpha", orderedResults[0].Value1, "First result should be Alpha");
        Assert.AreEqual("Another First", orderedResults[0].Value2, "First Alpha should be 'Another First'");
        Assert.AreEqual("Alpha", orderedResults[1].Value1, "Second result should be Alpha");
        Assert.AreEqual("First", orderedResults[1].Value2, "Second Alpha should be 'First'");
        Assert.AreEqual("Bravo", orderedResults[2].Value1, "Third result should be Bravo");
        Assert.AreEqual("Charlie", orderedResults[3].Value1, "Fourth result should be Charlie");

        TestContext.WriteLine("Query results (ordered by Value1, Value2):");
        foreach (var item in orderedResults)
        {
            TestContext.WriteLine($"  {item.Value1}: {item.Value2}");
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task UpdateDocument_WhenDocumentExists_ModifiesValues()
    {
        // Arrange
        var db = BuildTestDatabase();
        var originalValue = "Original Value";
        var updatedValue = "Updated Value";

        var entity = new TestCollection
        {
            Value1 = originalValue,
            Value2 = "Test",
            Date = DateTimeOffset.Now,
        };

        await db.Tests.InsertOneAsync(entity);
        Assert.IsNotNull(entity.TestId, "Document should have TestId after insert");

        // Act - Update the document
        var filter = Builders<TestCollection>.Filter.Eq(e => e.TestId, entity.TestId);
        var update = Builders<TestCollection>.Update.Set(e => e.Value1, updatedValue);
        var updateResult = await db.Tests.UpdateOneAsync(filter, update);

        // Assert
        Assert.AreEqual(1, updateResult.MatchedCount, "Should match one document");
        Assert.AreEqual(1, updateResult.ModifiedCount, "Should modify one document");

        // Verify the update
        var updatedEntity = await db.Tests.AsQueryable()
            .FirstOrDefaultAsync(e => e.TestId == entity.TestId);

        Assert.IsNotNull(updatedEntity, "Updated document should exist");
        Assert.AreEqual(updatedValue, updatedEntity.Value1, "Value1 should be updated");
        Assert.AreEqual(entity.Value2, updatedEntity.Value2, "Value2 should remain unchanged");

        TestContext.WriteLine($"Updated document {entity.TestId}: '{originalValue}' -> '{updatedValue}'");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task DeleteDocument_WhenDocumentExists_RemovesDocument()
    {
        // Arrange
        var db = BuildTestDatabase();
        var entity = new TestCollection
        {
            Value1 = "To Be Deleted",
            Value2 = "Test",
            Date = DateTimeOffset.Now,
        };

        await db.Tests.InsertOneAsync(entity);
        Assert.IsNotNull(entity.TestId, "Document should have TestId after insert");

        var initialCount = await db.Tests.AsQueryable().CountAsync();
        Assert.AreEqual(1, initialCount, "Should have one document before delete");

        // Act
        var filter = Builders<TestCollection>.Filter.Eq(e => e.TestId, entity.TestId);
        var deleteResult = await db.Tests.DeleteOneAsync(filter);

        // Assert
        Assert.AreEqual(1, deleteResult.DeletedCount, "Should delete one document");

        var remainingCount = await db.Tests.AsQueryable().CountAsync();
        Assert.AreEqual(0, remainingCount, "Should have no documents after delete");

        TestContext.WriteLine($"Deleted document with TestId: {entity.TestId}");
    }
}
