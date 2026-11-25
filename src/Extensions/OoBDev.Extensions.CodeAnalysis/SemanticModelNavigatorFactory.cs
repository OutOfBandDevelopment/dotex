using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis;

/// <summary>
/// Exposes a Roslyn semantic model as an XPath-navigable document: the syntax tree plus, under each node that binds to a
/// symbol, one <c>Symbol</c> element with the symbol facts as attributes.
/// </summary>
public static class SemanticModelNavigatorFactory
{
    /// <summary>Wraps the model in a navigable document.</summary>
    public static IXPathNavigable ToNavigable(this SemanticModel model, string? baseUri = null) =>
        new ExtensibleNavigator(model.AsNode(baseUri));

    /// <summary>Builds the root node of the model.</summary>
    public static INode AsNode(this SemanticModel pointer, string? baseUri = null)
    {
        ArgumentNullException.ThrowIfNull(pointer);
        return new ExtensibleElementNode(
            pointer.ToXName(),
            pointer,
            valueSelector: n => n is SemanticModelNode s ? GetValue(s.Node) : null,
            attributeSelector: n => n is SemanticModelNode s
                ? s.Node is ISymbol symbol ? GetSymbolAttributes(symbol) : SyntaxTreeNavigatorFactory.GetAttributes(s.Node)
                : null,
            childSelector: n => n switch
            {
                SemanticModelNode s => GetChildren(s),
                SemanticModel semantic => [(semantic.SyntaxTree.ToXName(), new SemanticModelNode(semantic, semantic.SyntaxTree))],
                _ => null,
            },
            preserveWhitespace: _ => false);
    }

    private static string? GetValue(object node) => node switch
    {
        SyntaxToken token => token.Text,
        SyntaxTrivia trivia => trivia.ToString(),
        ITypeParameterSymbol typeParameter => typeParameter.Name,
        INamespaceOrTypeSymbol namespaceOrType => namespaceOrType.ToString(),
        _ => null,
    };

    private static IEnumerable<(XName, object)>? GetChildren(SemanticModelNode s)
    {
        var semantic = s.Semantic;
        return s.Node switch
        {
            SyntaxTree tree => [(tree.GetRoot().ToXName(), new SemanticModelNode(semantic, tree.GetRoot()))],
            SyntaxNode node => NodeChildren(semantic, node),
            SyntaxToken token => token.GetAllTrivia().Select(i => (i.ToXName(), (object)new SemanticModelNode(semantic, i))),
            SyntaxTrivia { HasStructure: true } trivia => [(trivia.GetStructure()!.ToXName(), new SemanticModelNode(semantic, trivia.GetStructure()!))],
            ISymbol symbol => SymbolChildren(symbol, semantic),
            _ => null,
        };
    }

    private static IEnumerable<(XName, object)> NodeChildren(SemanticModel semantic, SyntaxNode node)
    {
        var symbol = semantic.GetSymbolInfo(node).Symbol;
        if (symbol != null)
            yield return (symbol.ToXName(), new SemanticModelNode(semantic, symbol));

        foreach (var child in node.ChildNodesAndTokens())
        {
            yield return child.IsNode
                ? (child.ToXName(), new SemanticModelNode(semantic, child.AsNode()!))
                : (child.ToXName(), new SemanticModelNode(semantic, child.AsToken()));
        }
    }

    private static IEnumerable<(XName, string?)> GetSymbolAttributes(ISymbol symbol)
    {
        var ns = symbol.ToNamespaceUri();
        yield return (ns + nameof(ISymbol.Name), symbol.Name);
        yield return (ns + nameof(ISymbol.Kind), symbol.Kind.ToString());
        yield return (ns + nameof(ISymbol.IsAbstract), symbol.IsAbstract.ToString());
        yield return (ns + nameof(ISymbol.IsExtern), symbol.IsExtern.ToString());
        yield return (ns + nameof(ISymbol.IsSealed), symbol.IsSealed.ToString());
        yield return (ns + nameof(ISymbol.IsOverride), symbol.IsOverride.ToString());
        yield return (ns + nameof(ISymbol.IsVirtual), symbol.IsVirtual.ToString());
        yield return (ns + nameof(ISymbol.IsStatic), symbol.IsStatic.ToString());
        yield return (ns + nameof(ISymbol.IsDefinition), symbol.IsDefinition.ToString());
        yield return (ns + "DisplayString", symbol.ToDisplayString());

        if (symbol is IParameterSymbol parameter)
        {
            yield return (ns + nameof(IParameterSymbol.RefKind), parameter.RefKind.ToString());
            yield return (ns + nameof(IParameterSymbol.IsParams), parameter.IsParams.ToString());
            yield return (ns + nameof(IParameterSymbol.IsOptional), parameter.IsOptional.ToString());
            yield return (ns + nameof(IParameterSymbol.IsThis), parameter.IsThis.ToString());
            yield return (ns + nameof(IParameterSymbol.IsDiscard), parameter.IsDiscard.ToString());
            yield return (ns + nameof(IParameterSymbol.NullableAnnotation), parameter.NullableAnnotation.ToString());
            yield return (ns + nameof(IParameterSymbol.Ordinal), parameter.Ordinal.ToString(CultureInfo.InvariantCulture));
            if (parameter.HasExplicitDefaultValue)
                yield return (ns + nameof(IParameterSymbol.ExplicitDefaultValue), parameter.ExplicitDefaultValue?.ToString());
        }
    }

    private static IEnumerable<(XName, object)> SymbolChildren(ISymbol symbol, SemanticModel semantic)
    {
        var ns = symbol.ToNamespaceUri();
        if (symbol is INamedTypeSymbol namedType)
        {
            if (namedType.IsTupleType)
                foreach (var part in namedType.TupleElements)
                    yield return (ns + "TupleField", new SemanticModelNode(semantic, part));
            foreach (var part in namedType.TypeArguments)
                yield return (ns + "TypeArgument", new SemanticModelNode(semantic, part));
        }
        else if (symbol is IParameterSymbol parameter)
        {
            foreach (var part in parameter.GetAttributes())
                if (part.AttributeClass != null)
                    yield return (ns + "Attribute", new SemanticModelNode(semantic, part.AttributeClass));
            yield return (ns + "Type", new SemanticModelNode(semantic, parameter.Type));
        }
    }
}
