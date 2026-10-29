using System;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

using CS = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;
using VB = Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions;

namespace OoBDev.Extensions.CodeAnalysis;

/// <summary>Maps Roslyn objects to the XML names used by the navigators.</summary>
public static class SyntaxNames
{
    /// <summary>The namespace prefix; the object category is appended (<c>Tree</c>, <c>Node</c>, <c>Token</c>, <c>Trivia</c>, <c>Semantic</c>, <c>Symbol</c>).</summary>
    public const string BaseUri = "oobdev:CodeAnalysis/";

    /// <summary>The Roslyn language name for C#.</summary>
    public const string CsLang = "C#";

    /// <summary>The Roslyn language name for Visual Basic.</summary>
    public const string VbLang = "Visual Basic";

    /// <summary>Gets the XML namespace for the category of the object.</summary>
    public static XNamespace ToNamespaceUri(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return BaseUri + obj switch
        {
            SyntaxTree => "Tree",
            SyntaxNode => "Node",
            SyntaxNodeOrToken nodeOrToken => nodeOrToken.IsNode ? "Node" : "Token",
            SyntaxToken => "Token",
            SyntaxTrivia => "Trivia",
            SemanticModel => "Semantic",
            ISymbol => "Symbol",
            _ => "#" + obj.GetType().Assembly.FullName,
        };
    }

    /// <summary>Gets the element name: the syntax kind for syntax objects, otherwise the type name.</summary>
    public static string ToLocalName(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
        return obj switch
        {
            SyntaxTree => "Tree",
            SyntaxNode node => node.Language switch { CsLang => CS.Kind(node).ToString(), VbLang => VB.Kind(node).ToString(), _ => null },
            SyntaxNodeOrToken nodeOrToken => nodeOrToken.Language switch { CsLang => CS.Kind(nodeOrToken).ToString(), VbLang => VB.Kind(nodeOrToken).ToString(), _ => null },
            SyntaxToken token => token.Language switch { CsLang => CS.Kind(token).ToString(), VbLang => VB.Kind(token).ToString(), _ => null },
            SyntaxTrivia trivia => trivia.Language switch { CsLang => CS.Kind(trivia).ToString(), VbLang => VB.Kind(trivia).ToString(), _ => null },
            SemanticModel => nameof(SemanticModel),
            ISymbol symbol => symbol.Kind.ToString(),
            _ => null,
        } ?? obj.GetType().Name;
    }

    /// <summary>Gets the full XML name (namespace and local name) of the object.</summary>
    public static XName ToXName(this object obj) => XName.Get(obj.ToLocalName(), obj.ToNamespaceUri().NamespaceName);
}
