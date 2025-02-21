namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Common pattern for declaring MongoDB Settings
/// </summary>
public interface IMongoSettings
{
    /// <summary>
    /// MongoDB Connection String
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Name of database to map for MongoDB
    /// </summary>
    public string DatabaseName { get; }
}
