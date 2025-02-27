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
public class MongoDBTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public void TestMethod1()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDatabaseOptions:DatabaseName" , "Test"},
                { "MongoDatabaseOptions:ConnectionString" , "mongodb://localhost:27017?collation={locale:'en_US',caseLevel:false,strength:2 }"},
            })
            ;
        var config = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddMongoServices(config, nameof(MongoDatabaseOptions));
        services.TryAddSystemExtensions(config, new());

        services.TryAddMongoDatabase<ITestMongoDatabase>();
        var provider = services.BuildServiceProvider();

        var db = provider.GetRequiredService<ITestMongoDatabase>();

        var entity = new TestCollection
        {
            //TestId = "655d21138c73243c786dbb72",
            Value1 = Guid.NewGuid().ToString(),
            Date = DateTimeOffset.Now,
            Value2 = "Test",
        };

        var filter = new FilterDefinitionBuilder<TestCollection>
        {

        }.Where(e => e.TestId != null && e.TestId == entity.TestId); //.Eq(e =>  e.TestId, entity.TestId);

        //if (entity.TestId == null)
        //{
        //    db.Tests.InsertOne(entity);
        //}
        //else
        //{
        //    var ret = db.Tests.ReplaceOne(filter, entity);
        //}

        //var insertOptions = new InsertOneOptions
        //{
        //};
        //db.Tests.InsertOne(entity);

        var replaceOptions = new FindOneAndReplaceOptions<TestCollection>
        {
            IsUpsert = true,
            ReturnDocument = ReturnDocument.After,
            BypassDocumentValidation = false,
        };
        var replaced = db.Tests.FindOneAndReplace(filter, entity, replaceOptions);

        //var updateDef = new UpdateDefinitionBuilder<TestCollection>()
        //    .Set(i => i.Value1, entity.Value1)
        //    ;
        //;
        //var udpateOptions = new FindOneAndUpdateOptions<TestCollection>
        //{
        //    IsUpsert = true,
        //    ReturnDocument = ReturnDocument.After,
        //};
        //var updated = db.Tests.FindOneAndUpdate(filter, updateDef, udpateOptions);

    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task TestMethod2()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDatabase:DatabaseName" , "Test"},
                { "MongoDatabase:ConnectionString" , "mongodb://localhost:27017?collation={locale:'en_US',caseLevel:false,strength:2 }"},
            })
            ;
        var config = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddMongoServices(config, "MongoDatabase");
        services.TryAddSystemExtensions(config, new());

        services.TryAddMongoDatabase<ITestMongoDatabase>();
        var provider = services.BuildServiceProvider();

        var db = provider.GetRequiredService<ITestMongoDatabase>();

        var entity = new TestCollection
        {
            //    TestId = "655d21138c73243c786dbb72",
            Value1 = Guid.NewGuid().ToString(),
            Date = DateTimeOffset.Now,
        };

        var filter = new FilterDefinitionBuilder<TestCollection>()
            .Where(e => e.TestId != null && e.TestId == entity.TestId);

        for (var x = 0; x < 10; x++)
        {
            entity.TestId = null;
            entity.Value1 = (x % 3) switch
            {
                0 => "UPPER",
                1 => "Upper",
                2 => "upper",
                _ => throw new NotSupportedException(),
            };
            entity.Value2 = $"{x} - {x % 3}";
            await db.Tests.InsertOneAsync(entity);
        }

        //var replaceOptions = new FindOneAndUpdateOptions<TestCollection>
        //{
        //    IsUpsert = true,
        //    ReturnDocument = ReturnDocument.After,
        //    BypassDocumentValidation = false,
        //};

        //var def = new UpdateDefinitionBuilder<TestCollection>().
        //    ;

        //var replaced = db.Tests.FindOneAndUpdate(
        //    filter, def, replaceOptions
        //    );

    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task TestMethod3()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDatabase:DatabaseName" , "Test"},
                { "MongoDatabase:ConnectionString" , "mongodb://localhost:27017?collation={locale:'en_US',caseLevel:false,strength:2 }"},
            })
            ;
        var config = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddMongoServices(config, "MongoDatabase");
        services.TryAddSystemExtensions(config, new());

        services.TryAddMongoDatabase<ITestMongoDatabase>();
        var provider = services.BuildServiceProvider();

        var db = provider.GetRequiredService<ITestMongoDatabase>();

        var query1 = await db.Tests.AsQueryable().OrderBy(e => e.Value1).ThenBy(e => e.Value2).Select(e => new { e.Value1, e.Value2 }).ToListAsync();
        // var query2 = db.Tests.AsQueryable().OrderBy(e => e.Value1, StringComparer.OrdinalIgnoreCase).Select(e => e.Value1).ToList();
        var query2 = db.Tests.AsQueryable().OrderBy(e => e.Value1.ToUpper()).ThenBy(e => e.Value2).Select(e => new { e.Value1, e.Value2 }).ToList();

        TestContext.WriteLine("-------------- 111111111111111111111111");
        TestContext.WriteLine(string.Join(";" + Environment.NewLine, query1));
        TestContext.WriteLine("-------------- 222222222222222222222222");
        TestContext.WriteLine(string.Join(";" + Environment.NewLine, query2));
    }
}
