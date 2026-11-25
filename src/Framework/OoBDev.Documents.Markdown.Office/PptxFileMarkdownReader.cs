using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace OoBDev.Documents.Markdown.Office;

/// <summary>
/// Reads a <c>.pptx</c> presentation as Markdown: one section per slide in presentation order, the title placeholder as the heading,
/// the other text paragraphs as bullets, then the speaker notes.
/// </summary>
public sealed class PptxFileMarkdownReader : IFileMarkdownReader
{
    private static readonly XNamespace _presentation = "http://schemas.openxmlformats.org/presentationml/2006/main";
    private static readonly XNamespace _drawing = "http://schemas.openxmlformats.org/drawingml/2006/main";

    /// <inheritdoc />
    public IReadOnlyCollection<string> Extensions { get; } = [".pptx", ".pptm"];

    /// <inheritdoc />
    public string Extractor => "pptx (Open XML)";

    /// <inheritdoc />
    public string Read(string path, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(path);
        using var package = new OpenXmlPackage(path);
        var presentation = package.Load("ppt/presentation.xml") ?? throw new NotSupportedException("The file has no presentation part.");
        var relationships = package.Relationships("ppt/presentation.xml");
        var builder = new StringBuilder();
        var number = 0;

        foreach (var slideId in presentation.Descendants(_presentation + "sldId"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = (string?)slideId.Attribute(OpenXmlPackage.RelationshipNamespace + "id");
            if (id is null || !relationships.TryGetValue(id, out var part))
            {
                continue;
            }

            number++;
            var slide = package.Load(part);
            if (slide is null)
            {
                continue;
            }

            var title = "";
            var bullets = new List<string>();
            foreach (var shape in slide.Descendants(_presentation + "sp"))
            {
                var placeholder = shape.Descendants(_presentation + "ph").FirstOrDefault();
                var paragraphs = Paragraphs(shape);
                if (placeholder is not null && (string?)placeholder.Attribute("type") is "title" or "ctrTitle")
                {
                    title = string.Join(" ", paragraphs);
                }
                else
                {
                    bullets.AddRange(paragraphs);
                }
            }

            builder.Append("## Slide ").Append(number);
            if (title.Length > 0)
            {
                builder.Append(": ").Append(title);
            }

            builder.AppendLine().AppendLine();
            foreach (var bullet in bullets)
            {
                builder.Append("- ").AppendLine(bullet);
            }

            var notes = Notes(package, part);
            if (notes.Count > 0)
            {
                builder.AppendLine().AppendLine("**Notes**").AppendLine();
                foreach (var note in notes)
                {
                    builder.AppendLine(note).AppendLine();
                }
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd() + Environment.NewLine;
    }

    private static List<string> Notes(OpenXmlPackage package, string slidePart)
    {
        foreach (var target in package.Relationships(slidePart).Values.Where(t => t.Contains("notesSlide", StringComparison.OrdinalIgnoreCase)))
        {
            var notes = package.Load(target);
            if (notes is null)
            {
                continue;
            }

            return notes.Descendants(_presentation + "sp")
                .Where(sp => (string?)sp.Descendants(_presentation + "ph").FirstOrDefault()?.Attribute("type") == "body")
                .SelectMany(Paragraphs)
                .ToList();
        }

        return [];
    }

    private static IEnumerable<string> Paragraphs(XElement shape) =>
        shape.Descendants(_drawing + "p")
            .Select(p => string.Concat(p.Descendants(_drawing + "t").Select(t => t.Value)).Trim())
            .Where(text => text.Length > 0);
}
