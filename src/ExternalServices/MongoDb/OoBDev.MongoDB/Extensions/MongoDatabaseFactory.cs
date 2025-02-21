using OoBDev.System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System;

namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Factory for creating MongoDB database instances.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MongoDatabaseFactory"/> class.
/// </remarks>
/// <param name="serviceProvider">The service provider for obtaining dependencies.</param>
public class MongoDatabaseFactory(IServiceProvider serviceProvider) : IMongoDatabaseFactory
{

    /// <summary>
    /// Creates a MongoDB database instance based on the provided settings.
    /// </summary>
    /// <typeparam name="TSettings">The type of MongoDB settings.</typeparam>
    /// <returns>The MongoDB database instance.</returns>
    public IMongoDatabase Create<TSettings>()
        where TSettings : class, IMongoSettings
    {
        var settings = serviceProvider.GetRequiredService<IOptions<TSettings>>();
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        return database;
    }

    /// <summary>
    /// Creates a proxied MongoDB database instance based on the provided settings.
    /// </summary>
    /// <typeparam name="TDatabase">The type of the MongoDB database.</typeparam>
    /// <typeparam name="TSettings">The type of MongoDB settings.</typeparam>
    /// <returns>The proxied MongoDB database instance.</returns>
    public TDatabase Create<TDatabase, TSettings>()
        where TSettings : class, IMongoSettings
    {
        var settings = serviceProvider.GetRequiredService<IOptions<TSettings>>();
        var jsonSerializer = serviceProvider.GetRequiredService<IJsonSerializer>();
        var database = Create<TSettings>();
        var proxy = MongoDispatchProxy.Create<TDatabase>(database, settings.Value, jsonSerializer);
        return proxy;
    }

    /// <summary>
    /// Static constructor to register serializers and conventions.
    /// </summary>
    static MongoDatabaseFactory()
    {
        // Registering DateTimeOffset serializer
        BsonSerializer.RegisterSerializer(
            new DateTimeOffsetSerializer(global::MongoDB.Bson.BsonType.DateTime)
        );

        // Registering conventions (camel case and BSON ObjectId)
        ConventionRegistry.Register("Camel case convention", new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new BsonObjectIdConvention(),
        }, t => true);
    }
}
