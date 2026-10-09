using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Reads the compiler generated XML documentation files that sit next to the application and answers
/// summary, remarks and parameter descriptions for types, properties and methods.
/// </summary>
public partial class XmlDocumentationProvider
{
    private readonly Dictionary<string, XElement> _members = new(StringComparer.Ordinal);

    /// <summary>
    /// Loads every <c>*.xml</c> documentation file from <see cref="AppContext.BaseDirectory"/>.
    /// </summary>
    /// <param name="logger">Logger used to report files that cannot be read.</param>
    public XmlDocumentationProvider(ILogger<XmlDocumentationProvider> logger)
    {
        foreach (var file in Directory.GetFiles(AppContext.BaseDirectory, "*.xml"))
        {
            try
            {
                var document = XDocument.Load(file);
                foreach (var member in document.Descendants("member"))
                {
                    var name = (string?)member.Attribute("name");
                    if (name != null)
                    {
                        _members[name] = member;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogDebug("{File}: {Message}", Path.GetFileName(file), e.Message);
            }
        }
    }

    /// <summary>
    /// Gets the summary text of a type, property or method.
    /// </summary>
    /// <param name="member">The documented member.</param>
    /// <returns>The summary, or <see langword="null"/> when undocumented.</returns>
    public string? GetSummary(MemberInfo member) => Text(Find(member)?.Element("summary"));

    /// <summary>
    /// Gets the remarks text of a type, property or method.
    /// </summary>
    /// <param name="member">The documented member.</param>
    /// <returns>The remarks, or <see langword="null"/> when undocumented.</returns>
    public string? GetRemarks(MemberInfo member) => Text(Find(member)?.Element("remarks"));

    /// <summary>
    /// Gets the description of a method parameter.
    /// </summary>
    /// <param name="method">The documented method.</param>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>The description, or <see langword="null"/> when undocumented.</returns>
    public string? GetParameter(MethodInfo method, string parameterName) =>
        Text(Find(method)?.Elements("param").FirstOrDefault(p => (string?)p.Attribute("name") == parameterName));

    private XElement? Find(MemberInfo member)
    {
        if (_members.Count == 0) return null;

        var declaring = member.DeclaringType is null ? null : TypeId(member.DeclaringType);
        switch (member)
        {
            case Type type:
                return _members.GetValueOrDefault($"T:{TypeId(type)}");
            case PropertyInfo property:
                return _members.GetValueOrDefault($"P:{declaring}.{property.Name}");
            case MethodInfo method:
                var prefix = $"M:{declaring}.{method.Name}";
                var parameters = method.GetParameters();
                var exact = parameters.Length == 0 ? prefix : $"{prefix}({string.Join(",", parameters.Select(p => TypeId(p.ParameterType)))})";
                if (_members.TryGetValue(exact, out var found)) return found;
                var candidates = _members.Where(m => m.Key == prefix || m.Key.StartsWith(prefix + "(", StringComparison.Ordinal)).ToList();
                return candidates.Count == 1 ? candidates[0].Value : null;
            default:
                return null;
        }
    }

    private static string TypeId(Type type)
    {
        if (type.IsByRef) return TypeId(type.GetElementType()!) + "@";
        if (type.IsArray) return TypeId(type.GetElementType()!) + "[]";
        if (type.IsGenericParameter) return (type.DeclaringMethod != null ? "``" : "`") + type.GenericParameterPosition;
        if (type.IsGenericType && !type.IsGenericTypeDefinition)
        {
            var definition = type.GetGenericTypeDefinition();
            var name = TypeId(definition);
            return name[..name.IndexOf('`')] + "{" + string.Join(",", type.GetGenericArguments().Select(TypeId)) + "}";
        }
        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    private static string? Text(XElement? element)
    {
        if (element == null) return null;

        var text = new StringBuilder();
        foreach (var node in element.Nodes())
        {
            switch (node)
            {
                case XText t:
                    text.Append(t.Value);
                    break;
                case XElement e when e.Name == "see" || e.Name == "seealso":
                    var target = (string?)e.Attribute("langword") ?? (string?)e.Attribute("cref") ?? e.Value;
                    var tail = target[(target.IndexOf(':') + 1)..];
                    text.Append('`').Append(tail.Contains('(') ? tail[..tail.IndexOf('(')] : tail).Append('`');
                    break;
                case XElement e when e.Name == "paramref" || e.Name == "typeparamref":
                    text.Append('`').Append((string?)e.Attribute("name")).Append('`');
                    break;
                case XElement e:
                    text.Append(e.Value);
                    break;
            }
        }

        var result = Whitespace().Replace(text.ToString(), " ").Trim();
        return result.Length == 0 ? null : result;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();
}
