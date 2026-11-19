using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace OoBDev.Documents.Markdown.Office;

/// <summary>
/// Reads an <c>.xlsx</c> workbook as Markdown: one section per worksheet and a table per sheet with the first row as the header.
/// Values are the stored cell values (formula results as last saved); no formatting or dates are interpreted.
/// </summary>
public sealed class XlsxFileMarkdownReader : IFileMarkdownReader
{
    private static readonly XNamespace _main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = [".xlsx", ".xlsm"];

    /// <inheritdoc />
    public string Extractor => "xlsx (Open XML)";

    /// <inheritdoc />
    public string Read(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var package = new OpenXmlPackage(path);
        var workbook = package.Load("xl/workbook.xml") ?? throw new NotSupportedException("The file has no workbook part.");
        var relationships = package.Relationships("xl/workbook.xml");
        var shared = ReadSharedStrings(package);
        var builder = new StringBuilder();

        foreach (var sheet in workbook.Descendants(_main + "sheet"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = (string?)sheet.Attribute("name") ?? "Sheet";
            var id = (string?)sheet.Attribute(OpenXmlPackage.RelationshipNamespace + "id");
            if (id is null || !relationships.TryGetValue(id, out var part))
            {
                continue;
            }

            var rows = ReadRows(package.Load(part), shared);
            builder.Append("## ").AppendLine(name).AppendLine();
            if (rows.Count == 0)
            {
                builder.AppendLine("_(empty)_").AppendLine();
                continue;
            }

            var width = rows.Max(r => r.Count);
            AppendRow(builder, rows[0], width);
            builder.Append('|').Append(string.Concat(Enumerable.Repeat(" --- |", width))).AppendLine();
            foreach (var row in rows.Skip(1))
            {
                AppendRow(builder, row, width);
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static void AppendRow(StringBuilder builder, List<string> row, int width)
    {
        builder.Append('|');
        for (var i = 0; i < width; i++)
        {
            var text = i < row.Count ? row[i] : "";
            builder.Append(' ').Append(text.Replace("|", "\\|", StringComparison.Ordinal).Replace("\r", " ", StringComparison.Ordinal).Replace("\n", "<br>", StringComparison.Ordinal)).Append(" |");
        }

        builder.AppendLine();
    }

    private static List<string> ReadSharedStrings(OpenXmlPackage package) =>
        package.Load("xl/sharedStrings.xml")?.Descendants(_main + "si").Select(si => string.Concat(si.Descendants(_main + "t").Select(t => t.Value))).ToList() ?? [];

    private static List<List<string>> ReadRows(XDocument? sheet, List<string> shared)
    {
        var rows = new List<List<string>>();
        if (sheet is null)
        {
            return rows;
        }

        foreach (var row in sheet.Descendants(_main + "row"))
        {
            var cells = new List<string>();
            foreach (var cell in row.Elements(_main + "c"))
            {
                var column = ColumnIndex((string?)cell.Attribute("r"), cells.Count);
                while (cells.Count < column)
                {
                    cells.Add("");
                }

                cells.Add(CellText(cell, shared));
            }

            if (cells.Any(c => c.Length > 0))
            {
                rows.Add(cells);
            }
        }

        return rows;
    }

    private static string CellText(XElement cell, List<string> shared)
    {
        var type = (string?)cell.Attribute("t");
        var value = (string?)cell.Element(_main + "v") ?? "";
        return type switch
        {
            "s" when int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var index) && index < shared.Count => shared[index],
            "inlineStr" => string.Concat(cell.Descendants(_main + "t").Select(t => t.Value)),
            "b" => value == "1" ? "TRUE" : "FALSE",
            _ => value,
        };
    }

    private static int ColumnIndex(string? reference, int fallback)
    {
        if (string.IsNullOrEmpty(reference))
        {
            return fallback;
        }

        var column = 0;
        foreach (var ch in reference.TakeWhile(char.IsLetter))
        {
            column = (column * 26) + (char.ToUpperInvariant(ch) - 'A' + 1);
        }

        return column == 0 ? fallback : column - 1;
    }
}
