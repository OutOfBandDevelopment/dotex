using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OoBDev.TestUtilities.Data;

/// <summary>
/// Splits CSV text into rows of fields, keeping the line each row started on for error messages.
/// </summary>
internal static class CsvRows
{
    /// <summary>Reads RFC 4180 style rows: commas, double-quoted fields with doubled quotes, line breaks inside quotes.</summary>
    /// <param name="text">The file content.</param>
    /// <param name="source">The file name used in error messages.</param>
    /// <returns>The non-blank rows with the 1-based line each started on.</returns>
    /// <exception cref="InvalidDataException">A quoted field is not closed or a quote follows field text.</exception>
    public static List<(int Line, string[] Fields)> Read(string text, string source)
    {
        var rows = new List<(int, string[])>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var line = 1;
        var rowLine = 1;
        var quoted = false;
        var wasQuoted = false;
        var i = 0;

        void EndField()
        {
            fields.Add(field.ToString());
            field.Clear();
        }

        void EndRow()
        {
            EndField();
            if (fields.Count > 1 || fields[0].Length > 0 || wasQuoted)
                rows.Add((rowLine, [.. fields]));
            fields.Clear();
            wasQuoted = false;
        }

        while (i < text.Length)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"' && i + 1 < text.Length && text[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else if (c == '"')
                {
                    quoted = false;
                }
                else
                {
                    if (c == '\n') line++;
                    field.Append(c);
                }
            }
            else if (c == '"')
            {
                if (field.Length > 0)
                    throw new InvalidDataException($"{source}:{line}: a quote starts inside field text.");
                quoted = true;
                wasQuoted = true;
            }
            else if (c == ',')
            {
                EndField();
            }
            else if (c == '\r' || c == '\n')
            {
                if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                EndRow();
                line++;
                rowLine = line;
            }
            else
            {
                field.Append(c);
            }

            i++;
        }

        if (quoted)
            throw new InvalidDataException($"{source}:{rowLine}: a quoted field is not closed.");
        if (field.Length > 0 || fields.Count > 0)
            EndRow();
        return rows;
    }
}
