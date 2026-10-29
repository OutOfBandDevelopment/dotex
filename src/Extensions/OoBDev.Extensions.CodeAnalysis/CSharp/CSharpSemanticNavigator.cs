using System.IO;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis.CSharp;

/// <summary>Parses C# source and binds it against an empty compilation, so only symbols that need no references resolve.</summary>
[FileExtension(".cs")]
public class CSharpSemanticNavigator : IToXPathNavigable
{
    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(string filePath) => Bind(CSharpSyntaxTree.ParseText(File.ReadAllText(filePath)));

    /// <inheritdoc />
    public IXPathNavigable? ToNavigable(Stream stream) => Bind(CSharpSyntaxTree.ParseText(SourceText.From(stream)));

    private static IXPathNavigable Bind(SyntaxTree syntax) =>
        CSharpCompilation.Create("temp").AddSyntaxTrees(syntax).GetSemanticModel(syntax).ToNavigable();
}
