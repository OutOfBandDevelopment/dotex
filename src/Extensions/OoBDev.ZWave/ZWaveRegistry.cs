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

    public static string? Manufacturer(ushort id) => _registry.Value.Manufacturers.GetValueOrDefault(id)?.Name;

    public static string? Product(ushort manufacturer, ushort type, ushort id) =>
        _registry.Value.Manufacturers.GetValueOrDefault(manufacturer)?.Products.GetValueOrDefault((type, id));

    public static string? CommandClass(byte id) => _registry.Value.CommandClasses.GetValueOrDefault(id);

    public static string? DeviceClass(byte generic, byte specific) =>
        _registry.Value.DeviceClasses.GetValueOrDefault((ushort)((generic << 8) | specific));

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

    private sealed record Registry(
        Dictionary<ushort, ManufacturerEntry> Manufacturers,
        Dictionary<byte, string> CommandClasses,
        Dictionary<ushort, string> DeviceClasses);

    private sealed record ManufacturerEntry(string Name)
    {
        public Dictionary<(ushort Type, ushort Id), string> Products { get; } = [];
    }
}
