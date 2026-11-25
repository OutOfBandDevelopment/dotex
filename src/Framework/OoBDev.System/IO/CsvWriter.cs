using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO;

/// <summary>
/// Writes a sequence of objects as CSV: one column per readable public instance property, every
/// field quoted, embedded quotes doubled, underscores in header names shown as spaces.
/// </summary>
public static class CsvWriter
{
    /// <summary>Writes <paramref name="source"/> to <paramref name="outStream"/>; the stream stays open.</summary>
    public static void WriteAsCsvTo<T>(this IEnumerable<T> source, Stream outStream, bool includeHeader = true)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(outStream);

        using var writer = new StreamWriter(outStream, new UTF8Encoding(false), 32 * 1024, leaveOpen: true);
        var properties = GetProperties<T>();
        if (includeHeader)
        {
            writer.WriteLine(BuildHeader(properties));
        }
        foreach (var item in source.Where(i => i is not null))
        {
            writer.WriteLine(BuildRow(properties, item!));
        }
    }

    /// <summary>Writes <paramref name="source"/> to <paramref name="outStream"/>; the stream stays open.</summary>
    public static async Task WriteAsCsvToAsync<T>(
        this IEnumerable<T> source,
        Stream outStream,
        bool includeHeader = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(outStream);

        var writer = new StreamWriter(outStream, new UTF8Encoding(false), 32 * 1024, leaveOpen: true);
        await using (writer.ConfigureAwait(false))
        {
            var properties = GetProperties<T>();
            if (includeHeader)
            {
                await writer.WriteLineAsync(BuildHeader(properties).AsMemory(), cancellationToken).ConfigureAwait(false);
            }
            foreach (var item in source.Where(i => i is not null))
            {
                await writer.WriteLineAsync(BuildRow(properties, item!).AsMemory(), cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private static PropertyInfo[] GetProperties<T>() =>
        [.. typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)];

    private static string BuildHeader(PropertyInfo[] properties) =>
        string.Join(',', properties.Select(p => Quote(p.Name.Replace('_', ' '))));

    private static string BuildRow(PropertyInfo[] properties, object item) =>
        string.Join(',', properties.Select(p => Quote(Format(p.GetValue(item)))));

    private static string Format(object? value) =>
        value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value?.ToString() ?? string.Empty;

    private static string Quote(string value) => $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
