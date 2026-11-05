using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OoBDev.ImageMask.Cli;

/// <summary>The zones file: a list of bounding boxes, optionally scoped to image names.</summary>
public sealed class MaskDefinition
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true,
    };

    /// <summary>The bounding boxes to paint over.</summary>
    public IList<MaskZone> Zones { get; set; } = [];

    /// <summary>Reads a definition from JSON text.</summary>
    public static MaskDefinition Parse(string json)
        => JsonSerializer.Deserialize<MaskDefinition>(json, _options) ?? new MaskDefinition();

    /// <summary>Reads a definition from a file.</summary>
    public static MaskDefinition Load(string path) => Parse(File.ReadAllText(path));

    /// <summary>Writes the definition as indented JSON.</summary>
    public string ToJson() => JsonSerializer.Serialize(this, _options);
}
