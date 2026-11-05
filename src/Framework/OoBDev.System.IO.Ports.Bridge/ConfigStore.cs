using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>
/// Reads and writes the "Bridges" section of a JSON config file, leaving every other
/// section (Logging, etc.) untouched on save.
/// </summary>
public sealed class ConfigStore
{
    private const string BridgesKey = "Bridges";
    private const string MdnsKey = "Mdns";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly JsonNodeOptions NodeOptions = new();

    private static readonly JsonDocumentOptions DocumentOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public ConfigStore(string filePath)
    {
        FilePath = Path.GetFullPath(filePath);
    }

    public static string DefaultPath => Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    public string FilePath { get; }

    public Ser2NetConfig Load()
    {
        if (!File.Exists(FilePath))
        {
            return new Ser2NetConfig();
        }

        var root = ParseRoot();
        var bridges = root[BridgesKey]?.Deserialize<List<BridgeOptions>>(SerializerOptions) ?? [];
        var mdns = root[MdnsKey]?.Deserialize<MdnsOptions>(SerializerOptions) ?? new MdnsOptions();
        return new Ser2NetConfig { Bridges = bridges, Mdns = mdns };
    }

    public void Save(Ser2NetConfig config)
    {
        var root = File.Exists(FilePath) ? ParseRoot() : new JsonObject();
        root[BridgesKey] = JsonSerializer.SerializeToNode(config.Bridges, SerializerOptions);
        root[MdnsKey] = JsonSerializer.SerializeToNode(config.Mdns, SerializerOptions);

        var directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(FilePath, root.ToJsonString(SerializerOptions));
    }

    private JsonObject ParseRoot() =>
        JsonNode.Parse(File.ReadAllText(FilePath), NodeOptions, DocumentOptions) as JsonObject
        ?? throw new InvalidDataException($"{FilePath} does not contain a JSON object.");
}
