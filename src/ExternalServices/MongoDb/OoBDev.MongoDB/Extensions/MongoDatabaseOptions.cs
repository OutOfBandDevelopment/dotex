namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Default connection information for MongoDB databases. Duplicating this class with a different
/// value will allow for declaring secondary connection configurations.
/// </summary>
public class MongoDatabaseOptions : IMongoSettings
{
    /// <summary>
    /// Gets or sets the connection string for the MongoDB database.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the MongoDB database.
    /// </summary>
    public required string DatabaseName { get; set; }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => new
    {
        ConnectionString,
        DatabaseName,
    }.ToString() ?? "";
}
