using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis;

/// <summary>Exposes a Roslyn syntax tree as an XPath-navigable document.</summary>
public static class SyntaxTreeNavigatorFactory
{
    /// <summary>Wraps the tree in a navigable document.</summary>
    public static IXPathNavigable ToNavigable(this SyntaxTree tree, string? baseUri = null) =>
        new ExtensibleNavigator(tree.AsNode(baseUri));

    /// <summary>Builds the root node of the tree. Elements are named by syntax kind; tokens carry their text as the value and trivia hang under tokens.</summary>
    public static INode AsNode(this SyntaxTree pointer, string? baseUri = null)
    {
        ArgumentNullException.ThrowIfNull(pointer);
        return new ExtensibleElementNode(
            pointer.ToXName(),
            pointer,
            valueSelector: n => n is SyntaxToken token ? token.Text : null,
            attributeSelector: n => GetAttributes(n),
            childSelector: n => GetChildren(n),
            preserveWhitespace: _ => false);
    }

    internal static IEnumerable<(XName, string?)>? GetAttributes(object item)
    {
        switch (item)
        {
            case SyntaxTree tree:
                var root = tree.GetRoot();
                return
                [
                    (XName.Get(nameof(root.RawKind)), root.RawKind.ToString(CultureInfo.InvariantCulture)),
                    (XName.Get(nameof(root.Language)), root.Language),
                    .. LocationAttributes(root.GetLocation()),
                ];
            case SyntaxNode node:
                return [(XName.Get(nameof(node.RawKind)), node.RawKind.ToString(CultureInfo.InvariantCulture)), .. LocationAttributes(node.GetLocation())];
            case SyntaxToken token:
                return [(XName.Get(nameof(token.RawKind)), token.RawKind.ToString(CultureInfo.InvariantCulture)), .. LocationAttributes(token.GetLocation())];
            case SyntaxTrivia trivia:
                return [(XName.Get(nameof(trivia.RawKind)), trivia.RawKind.ToString(CultureInfo.InvariantCulture)), .. LocationAttributes(trivia.GetLocation())];
            default:
                return null;
        }
    }

    internal static IEnumerable<(XName, string?)> LocationAttributes(Location? location)
    {
        yield return (XName.Get($"{nameof(Location)}.{nameof(TextSpan.Start)}"), location?.SourceSpan.Start.ToString(CultureInfo.InvariantCulture));
        yield return (XName.Get($"{nameof(Location)}.{nameof(TextSpan.End)}"), location?.SourceSpan.End.ToString(CultureInfo.InvariantCulture));
    }

    private static IEnumerable<(XName, object)>? GetChildren(object item) => item switch
    {
        SyntaxTree tree => [(tree.GetRoot().ToXName(), tree.GetRoot())],
        SyntaxNode node => node.ChildNodesAndTokens().Select(i => (i.ToXName(), i.IsNode ? (object)i.AsNode()! : i.AsToken())),
        SyntaxToken token => token.GetAllTrivia().Select(i => (i.ToXName(), (object)i)),
        SyntaxTrivia { HasStructure: true } trivia => [(trivia.GetStructure()!.ToXName(), trivia.GetStructure()!)],
        SyntaxTrivia => null,
        _ => throw new NotSupportedException(item.GetType().FullName),
    };
}
