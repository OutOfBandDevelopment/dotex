namespace OoBDev.System.Text.Json.Serialization;

/// <summary>
/// Default serializer for BSON (Binary JSON).
/// </summary>
public class DefaultBsonSerializer : DefaultJsonSerializer, IBsonSerializer
{
    /// <summary>
    /// The default content type for BSON.
    /// </summary>
    public new const string DefaultContentType = "application/json";

    /// <summary>
    /// Gets the content type for BSON, which is "application/json".
    /// </summary>
    public override string ContentType => DefaultContentType;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultBsonSerializer"/> class.
    /// </summary>
    public DefaultBsonSerializer() =>
        // Set the type information resolver to a new instance of BsonTypeInfoResolver.
        _options.TypeInfoResolver = new BsonTypeInfoResolver();
}
