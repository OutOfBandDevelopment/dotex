using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OoBDev.ZWave;

/// <summary>
/// Id-to-name tables from the embedded data files: manufacturers with their products, command classes and device classes.
/// The files are factual identifier lists (Z-Wave Alliance registry ids as collected by the OpenZWave and PyZwaver projects);
/// none of their code is used. Loaded once on first use.
/// </summary>
internal static class ZWaveRegistry
{
    private static readonly Lazy<Registry> _registry = new(Load);
    private static readonly Lazy<ConfigData> _configs = new(LoadConfigs);

    public static string? Manufacturer(ushort id) => _registry.Value.Manufacturers.GetValueOrDefault(id)?.Name;

    public static string? Product(ushort manufacturer, ushort type, ushort id) =>
        _registry.Value.Manufacturers.GetValueOrDefault(manufacturer)?.Products.GetValueOrDefault((type, id));

    public static string? CommandClass(byte id) => _registry.Value.CommandClasses.GetValueOrDefault(id);

    public static string? DeviceClass(byte generic, byte specific) =>
        _registry.Value.DeviceClasses.GetValueOrDefault((ushort)((generic << 8) | specific));

    public static IReadOnlyList<ZWaveConfigParameter> ConfigParameters(ushort manufacturer, ushort type, ushort id)
    {
        var key = $"{manufacturer:X4}:{type:X4}:{id:X4}";
        return _configs.Value.Products.TryGetValue(key, out var path) && _configs.Value.Configs.TryGetValue(path, out var parameters) ? parameters : [];
    }

    private static ConfigData LoadConfigs()
    {
        using var doc = Read("product-configs.json");
        var products = doc.RootElement.GetProperty("products").EnumerateObject()
            .ToDictionary(p => p.Name, p => p.Value.GetString() ?? string.Empty, StringComparer.Ordinal);
        var configs = new Dictionary<string, IReadOnlyList<ZWaveConfigParameter>>(StringComparer.Ordinal);
        foreach (var c in doc.RootElement.GetProperty("configs").EnumerateObject())
        {
            configs[c.Name] = [.. c.Value.GetProperty("parameters").EnumerateArray().Select(ReadParameter)];
        }

        return new ConfigData(products, configs);
    }

    private static ZWaveConfigParameter ReadParameter(JsonElement e)
    {
        var type = Text(e, "type") ?? string.Empty;
        var size = e.GetProperty("size").GetInt32();
        if (size == 0)
        {
            size = type switch { "short" => 2, "int" => 4, _ => 1 };
        }

        return new ZWaveConfigParameter(
            e.GetProperty("index").GetInt32(),
            Text(e, "label") ?? string.Empty,
            size,
            Long(e, "min"),
            Long(e, "max"),
            Long(e, "default"),
            Text(e, "help"),
            e.TryGetProperty("items", out var items)
                ? [.. items.EnumerateArray().Select(i => new KeyValuePair<long, string>(i.GetProperty("value").GetInt64(), Text(i, "label") ?? string.Empty))]
                : []);
    }

    // The generated data can hold an array where one value was expected (a repeated attribute in the source); take the first.
    private static string? Text(JsonElement e, string name) =>
        !e.TryGetProperty(name, out var v) ? null : v.ValueKind == JsonValueKind.Array ? (v.GetArrayLength() > 0 ? v[0].GetString() : null) : v.GetString();

    private static long? Long(JsonElement e, string name) => e.TryGetProperty(name, out var v) ? v.GetInt64() : null;

    private static Registry Load()
    {
        var manufacturers = new Dictionary<ushort, ManufacturerEntry>();
        using (var doc = Read("manufacturers.json"))
        {
            foreach (var m in doc.RootElement.EnumerateArray())
            {
                var manufacturerId = Hex(m.GetProperty("id").GetString());
                var name = m.GetProperty("name").GetString() ?? string.Empty;
                if (!manufacturers.TryGetValue(manufacturerId, out var entry))
                {
                    entry = new ManufacturerEntry(name);
                    manufacturers[manufacturerId] = entry;
                }

                if (m.TryGetProperty("products", out var products))
                {
                    foreach (var p in products.EnumerateArray())
                    {
                        entry.Products[(Hex(p.GetProperty("type").GetString()), Hex(p.GetProperty("id").GetString()))] =
                            p.GetProperty("name").GetString() ?? string.Empty;
                    }
                }
            }
        }

        return new Registry(
            manufacturers,
            ReadMap("command-classes.json", v => (byte)v),
            ReadMap("device-classes.json", v => v));
    }

    private static Dictionary<TKey, string> ReadMap<TKey>(string name, Func<ushort, TKey> key)
        where TKey : notnull
    {
        using var doc = Read(name);
        var map = new Dictionary<TKey, string>();
        foreach (var p in doc.RootElement.EnumerateObject())
        {
            map[key(Hex(p.Name))] = p.Value.GetString() ?? string.Empty;
        }

        return map;
    }

    private static ushort Hex(string? text) =>
        ushort.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v) ? v : (ushort)0;

    private static JsonDocument Read(string name)
    {
        using Stream stream = typeof(ZWaveRegistry).Assembly.GetManifestResourceStream($"OoBDev.ZWave.Data.{name}")
            ?? throw new InvalidOperationException($"Embedded Z-Wave data '{name}' is missing.");
        return JsonDocument.Parse(stream);
    }

    private sealed record ConfigData(Dictionary<string, string> Products, Dictionary<string, IReadOnlyList<ZWaveConfigParameter>> Configs);

    private sealed record Registry(
        Dictionary<ushort, ManufacturerEntry> Manufacturers,
        Dictionary<byte, string> CommandClasses,
        Dictionary<ushort, string> DeviceClasses);

    private sealed record ManufacturerEntry(string Name)
    {
        public Dictionary<(ushort Type, ushort Id), string> Products { get; } = [];
    }
}
