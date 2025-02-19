using MongoDB.Driver;

namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// provide a centralized means to create Atlas Vector Search
/// </summary>
public interface IMongoVectorSearch
{
    /// <summary>
    /// Creates a vector search instance in the MongoDB database using the specified settings.
    /// </summary>
    /// <typeparam name="TSettings">The settings class implementing <see cref="IMongoSettings"/> for configuring the database.</typeparam>
    /// <returns>An <see cref="IMongoDatabase"/> instance configured for vector search.</returns>
    IMongoDatabase CreateVectorSearch<TSettings>()
        where TSettings : class, IMongoSettings;
}
