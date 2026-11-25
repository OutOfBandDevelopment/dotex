using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.TestUtilities.Data;

/// <summary>
/// Supplies the rows of a CSV file as the arguments of a data-driven test. The first line holds column names; each column
/// is matched to the test method parameter of the same name (case-insensitive), so the column order does not matter.
/// Values convert with the invariant culture: enums by name, hex text to <c>byte[]</c>, and empty text to <see langword="null"/>
/// for nullable value types (a string stays empty). Each row is one test case named after the method, the file and the line.
/// </summary>
/// <example>
/// <code>
/// [TestMethod, TestCategory(TestCategories.Unit), CsvData("Data/checksums.csv")]
/// public void Checksum_Bytes_MatchesExpected(string hex, int expected) { ... }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CsvDataAttribute : Attribute, ITestDataSource
{
    private readonly Dictionary<object?[], int> _lines = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Initializes the attribute.
    /// </summary>
    /// <param name="path">The CSV file, relative to the test assembly folder (mark it copy to output) or absolute.</param>
    public CsvDataAttribute(string path) => FilePath = path;

    /// <summary>Gets the CSV file path as given.</summary>
    public string FilePath { get; }

    /// <inheritdoc />
    public IEnumerable<object?[]> GetData(MethodInfo methodInfo)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);
        var file = Resolve(methodInfo);
        var name = Path.GetFileName(file);
        var rows = CsvRows.Read(File.ReadAllText(file), name);
        if (rows.Count == 0)
            throw new InvalidDataException($"{name}: the file has no header line.");

        var header = rows[0].Fields.Select(h => h.Trim()).ToArray();
        var parameters = methodInfo.GetParameters();
        var columnOf = new int[parameters.Length];
        for (var p = 0; p < parameters.Length; p++)
        {
            columnOf[p] = Array.FindIndex(header, h => string.Equals(h, parameters[p].Name, StringComparison.OrdinalIgnoreCase));
            if (columnOf[p] < 0)
                throw new InvalidDataException($"{name}:{rows[0].Line}: no column for parameter '{parameters[p].Name}' (columns: {string.Join(", ", header)}).");
        }

        var result = new List<object?[]>();
        foreach (var (line, fields) in rows.Skip(1))
        {
            if (fields.Length != header.Length)
                throw new InvalidDataException($"{name}:{line}: {fields.Length} fields but the header has {header.Length}.");

            var args = new object?[parameters.Length];
            for (var p = 0; p < parameters.Length; p++)
            {
                try
                {
                    args[p] = ConvertValue(fields[columnOf[p]], parameters[p].ParameterType);
                }
                catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentException)
                {
                    throw new InvalidDataException($"{name}:{line}: column '{parameters[p].Name}' value '{fields[columnOf[p]]}' is not a {parameters[p].ParameterType.Name}.", ex);
                }
            }

            _lines[args] = line;
            result.Add(args);
        }

        return result;
    }

    /// <inheritdoc />
    public string? GetDisplayName(MethodInfo methodInfo, object?[]? data)
    {
        ArgumentNullException.ThrowIfNull(methodInfo);
        return data is not null && _lines.TryGetValue(data, out var line)
            ? $"{methodInfo.Name} ({Path.GetFileName(FilePath)}:{line})"
            : methodInfo.Name;
    }

    private string Resolve(MethodInfo methodInfo)
    {
        if (Path.IsPathRooted(FilePath))
            return FilePath;

        var assemblyFolder = Path.GetDirectoryName(methodInfo.DeclaringType!.Assembly.Location) ?? AppContext.BaseDirectory;
        var candidate = Path.Combine(assemblyFolder, FilePath);
        return File.Exists(candidate)
            ? candidate
            : throw new FileNotFoundException($"CSV data file '{FilePath}' was not found in {assemblyFolder}; set CopyToOutputDirectory on it.", candidate);
    }

    private static object? ConvertValue(string text, Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (text.Length == 0 && underlying is not null)
            return null;

        var target = underlying ?? type;
        if (target == typeof(string)) return text;
        if (target.IsEnum) return Enum.Parse(target, text, ignoreCase: true);
        if (target == typeof(Guid)) return Guid.Parse(text);
        if (target == typeof(byte[])) return Convert.FromHexString(text);
        if (target == typeof(TimeSpan)) return TimeSpan.Parse(text, CultureInfo.InvariantCulture);
        if (target == typeof(DateTime)) return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (target == typeof(bool)) return bool.Parse(text);
        return Convert.ChangeType(text, target, CultureInfo.InvariantCulture);
    }
}
