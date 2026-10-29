using System.IO;
using System.Xml.XPath;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis.VisualBasic;

/// <summary>Parses Visual Basic source into a navigable syntax tree.</summary>
[FileExtension(".vb")]
public class VisualBasicNavigator : IToXPathNavigable
{
    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(string filePath) =>
        VisualBasicSyntaxTree.ParseText(File.ReadAllText(filePath)).ToNavigable(filePath);

    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(Stream stream) =>
        VisualBasicSyntaxTree.ParseText(SourceText.From(stream)).ToNavigable();
}
