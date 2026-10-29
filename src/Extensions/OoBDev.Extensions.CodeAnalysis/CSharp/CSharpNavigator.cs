using System.IO;
using System.Xml.XPath;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis.CSharp;

/// <summary>Parses C# source into a navigable syntax tree.</summary>
[FileExtension(".cs")]
public class CSharpNavigator : IToXPathNavigable
{
    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(string filePath) =>
        CSharpSyntaxTree.ParseText(File.ReadAllText(filePath)).ToNavigable(filePath);

    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(Stream stream) =>
        CSharpSyntaxTree.ParseText(SourceText.From(stream)).ToNavigable();
}
