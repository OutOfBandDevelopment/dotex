using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>Reads the <c>UsbBridges</c> array of the shared ser2net-win JSON config file.</summary>
public static class UsbBridgeConfig
{
    private const string Key = "UsbBridges";

    private static readonly JsonSerializerOptions _options = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>Returns the configured USB bridges, or none when the file or the section is missing.</summary>
    public static IReadOnlyList<UsbBridgeOptions> Load(string path)
    {
        if (!File.Exists(path))
            return [];
        var root = JsonNode.Parse(File.ReadAllText(path), null, new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true }) as JsonObject;
        return root?[Key]?.Deserialize<List<UsbBridgeOptions>>(_options) ?? [];
    }
}
