using Markdig.Parsers;
using Markdig.Syntax;
using System;

namespace OoBDev.System.Text.Markdown;

/// <summary>
/// Represents a PlantUML code block in Markdown documents.
/// Extends FencedCodeBlock to provide PlantUML-specific functionality.
/// </summary>
/// <param name="parser">The block parser that created this block.</param>
public class PlantUmlBlock(BlockParser parser) : FencedCodeBlock(parser)
{
    /// <summary>
    /// Gets the PlantUML script content as a single string.
    /// </summary>
    /// <returns>The PlantUML script with lines joined by newlines.</returns>
    public string GetScript() => string.Join(Environment.NewLine, Lines);
}
