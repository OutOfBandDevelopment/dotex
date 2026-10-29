using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using OoBDev.System.Reflection;

namespace OoBDev.Extensions.CodeAnalysis.DacFx;

/// <summary>
/// Builds an XPath node tree for a SQL Server model: top-level objects are children of the root, an object's children follow,
/// and its parsed script (<c>AST</c>) and source text (<c>SCRIPT</c>) hang beneath it.
/// </summary>
/// <param name="sqlModel">The model to expose.</param>
public class DacPacElementNodeBuilder(TSqlModel sqlModel) : ReflectionElementNodeBuilder(sqlModel, true, true)
{
    private const string NoNamespace = "";

    private static readonly HashSet<string> ExcludedProperties =
    [
        "Item", "Count",
        "StartOffset", "FragmentLength", "StartLine", "StartColumn", "FirstTokenIndex", "LastTokenIndex",
        "OwningType", "ObjectType",
    ];

    private readonly HashSet<object> _collected = [];

    /// <inheritdoc />
    protected override IEnumerable<(XName name, object child)>? ChildSelector(object model) =>
        (model switch
        {
            TSqlModel sql => ModelChildren(sql),
            TSqlObject sql => ObjectChildren(sql),
            Identifier => null,
            _ => [],
        })?.Concat(base.ChildSelector(model) ?? []);

    /// <inheritdoc />
    protected override IEnumerable<(XName name, string? value)>? AttributeSelector(object model) =>
        RefIdAttribute(model).Concat(
            model switch
            {
                TSqlModel => null,
                TSqlObject sql => [(XName.Get(nameof(sql.Name), NoNamespace), sql.Name.ToString())],
                Identifier ident => [(XName.Get(nameof(ident.QuoteType), NoNamespace), ident.QuoteType.ToString())],
                Literal literal =>
                [
                    (XName.Get(nameof(literal.LiteralType), NoNamespace), literal.LiteralType.ToString()),
                    (XName.Get(nameof(literal.Collation), NoNamespace), literal.Collation?.ToString()),
                ],
                Type => null,
                _ => base.AttributeSelector(model),
            } ?? []);

    /// <inheritdoc />
    protected override bool AllowNavigate(object model, PropertyInfo property)
    {
        if (!base.AllowNavigate(model, property)) return false;
        if (property.PropertyType.IsInstanceOfType(typeof(ObjectIdentifier))) return false;
        if (ExcludedProperties.Contains(property.Name)) return false;
        if (_collected.Contains(model)) return false;

        _collected.Add((model, property));
        return true;
    }

    /// <inheritdoc />
    protected override string? ValueSelector(object? model) =>
        model switch
        {
            TSqlModel or TSqlObject => null,
            Identifier ident => ident.ToString(),
            Literal literal => literal.Value,
            Type type => type.ToString(),
            _ => base.ValueSelector(model),
        };

    private static IEnumerable<(XName name, string? value)> RefIdAttribute(object model)
    {
        yield return (XName.Get("ref-id", NoNamespace), model?.GetHashCode().ToString(global::System.Globalization.CultureInfo.InvariantCulture));
    }

    private static IEnumerable<(XName name, object child)> ModelChildren(TSqlModel model) =>
        model.GetObjects(DacQueryScopes.Default | DacQueryScopes.SameDatabase)
            .Where(i => i.Name.Parts.Count == 1)
            .Select(i => (XName.Get(i.ObjectType.Name, NoNamespace), (object)i));

    private static IEnumerable<(XName name, object child)> ObjectChildren(TSqlObject model)
    {
        foreach (var i in model.GetChildren())
            yield return (XName.Get(i.ObjectType.Name, NoNamespace), i);

        if (model.TryGetAst(out var ast))
            yield return (XName.Get("AST", NoNamespace), ast);

        if (model.TryGetScript(out var script))
            yield return (XName.Get("SCRIPT", NoNamespace), script);
    }
}
