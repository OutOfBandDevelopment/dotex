using MongoDB.Driver;

namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// provide a centralized means to created MongoDB instances
/// </summary>
public interface IMongoDatabaseFactory
{
    /// <summary>
    /// factory method to create a <seealso cref="IMongoDatabase"/>  instance.
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    /// <returns></returns>
    IMongoDatabase Create<TSettings>()
        where TSettings : class, IMongoSettings;

    /// <summary>
    /// factory method to create a MongoDB Database abstraction.
    /// </summary>
    /// <typeparam name="TDatabase"></typeparam>
    /// <typeparam name="TSettings"></typeparam>
    /// <returns></returns>
    TDatabase Create<TDatabase, TSettings>()
        where TSettings : class, IMongoSettings;
}
