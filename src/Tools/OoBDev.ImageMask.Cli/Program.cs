using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace OoBDev.ImageMask.Cli;

/// <summary>image-mask: sync a new capture into the committed unmasked image only when it differs after masking; paint redaction blocks over zones listed in a JSON file, or compare two images with the zones masked. Exit code 0 equal, 1 different, 2 error.</summary>
internal static class Program
{
    private const string _usage = """
        image-mask mask --input a.png --zones zones.json --output masked.png
        image-mask diff --expected a.png --actual b.png [--zones zones.json] [--tolerance 0] [--max-pixels 0] [--diff-out diff.png]
        image-mask sync --new new.png --committed guide.png [--zones zones.json] [--tolerance 0] [--max-pixels 0]
        Zones file: { "zones": [ { "name": "clock", "match": "tui-*.png", "x": 60, "y": 0, "width": 80, "height": 16, "color": "#FF0000" } ] }
        """;

    private static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] is not ("mask" or "diff" or "sync"))
        {
            Console.Error.WriteLine(_usage);
            return 2;
        }

        var config = new ConfigurationBuilder().AddCommandLine(args[1..]).Build();
        try
        {
            return args[0] switch { "mask" => Mask(config), "diff" => Diff(config), _ => Sync(config) };
        }
        catch (Exception ex) when (ex is IOException or FormatException or InvalidDataException or System.Text.Json.JsonException or ArgumentException)
        {
            Console.Error.WriteLine(ex.Message);
            return 2;
        }
    }

    private static int Mask(IConfiguration config)
    {
        var input = Required(config, "input");
        var output = Required(config, "output");
        var definition = MaskDefinition.Load(Required(config, "zones"));

        using var bitmap = ImageMasker.Load(input);
        ImageMasker.Apply(bitmap, definition, Path.GetFileName(input));
        ImageMasker.SavePng(bitmap, output);
        return 0;
    }

    private static int Diff(IConfiguration config)
    {
        var expectedPath = Required(config, "expected");
        var actualPath = Required(config, "actual");
        var definition = config["zones"] is { Length: > 0 } zones ? MaskDefinition.Load(zones) : new MaskDefinition();
        var tolerance = config.GetValue("tolerance", 0);
        var maxPixels = config.GetValue("max-pixels", 0L);

        using var expected = ImageMasker.Load(expectedPath);
        using var actual = ImageMasker.Load(actualPath);
        ImageMasker.Apply(expected, definition, Path.GetFileName(expectedPath));
        ImageMasker.Apply(actual, definition, Path.GetFileName(actualPath));

        var result = ImageComparer.Compare(expected, actual, tolerance, out var diff);
        using (diff)
        {
            if (diff is not null && config["diff-out"] is { Length: > 0 } diffOut)
                ImageMasker.SavePng(diff, diffOut);
        }

        Console.WriteLine(result.SizeMatches
            ? $"{result.DifferentPixels} of {result.TotalPixels} pixels differ"
            : "image sizes differ");
        return result.IsMatch(maxPixels) ? 0 : 1;
    }

    private static int Sync(IConfiguration config)
    {
        var newPath = Required(config, "new");
        var committedPath = Required(config, "committed");
        var definition = config["zones"] is { Length: > 0 } zones ? MaskDefinition.Load(zones) : new MaskDefinition();
        var tolerance = config.GetValue("tolerance", 0);
        var maxPixels = config.GetValue("max-pixels", 0L);

        if (ImageSync.Run(newPath, committedPath, definition, tolerance, maxPixels))
        {
            Console.WriteLine("updated (unmasked image copied)");
            return 0;
        }

        Console.WriteLine("unchanged (equal once masked, committed image kept)");
        return 0;
    }

    private static string Required(IConfiguration config, string key)
        => config[key] is { Length: > 0 } value ? value : throw new ArgumentException($"Missing --{key}.", key);
}
