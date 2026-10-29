using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Build.Logging.StructuredLogger;
using OoBDev.System.Xml.XPath;

namespace OoBDev.Extensions.CodeAnalysis.StructuredLog;

/// <summary>Exposes an MSBuild structured log as an XPath-navigable document. Elements are named by node type; facts are attributes.</summary>
public static class StructuredLogNavigatorFactory
{
    /// <summary>Wraps the build in a navigable document.</summary>
    public static IXPathNavigable ToNavigable(this Build tree) =>
        new ExtensibleNavigator(tree.AsNode());

    /// <summary>Builds the root node of the build.</summary>
    public static INode AsNode(this Build build)
    {
        ArgumentNullException.ThrowIfNull(build);
        return new ExtensibleElementNode(
            "Build",
            build,
            valueSelector: n => n as string,
            attributeSelector: GetAttributes,
            childSelector: GetChildren);
    }

    private static IEnumerable<(XName, string?)>? GetAttributes(object item) => item switch
    {
        Build log => Attributes(log, ("Duration", log.Duration), ("EndTime", log.EndTime), ("Id", log.Id), ("LogFilePath", log.LogFilePath),
            ("Name", log.Name), ("NodeId", log.NodeId), ("StartTime", log.StartTime), ("Succeeded", log.Succeeded)),
        Property log => Attributes(log, ("Name", log.Name), ("ShortenedValue", log.ShortenedValue), ("Value", log.Value)),
        Folder log => Attributes(log, ("Name", log.Name)),
        Item log => Attributes(log, ("Name", log.Name), ("ShortenedName", log.ShortenedName), ("Text", log.Text)),
        ProjectEvaluation log => Attributes(log, ("Duration", log.Duration), ("EndTime", log.EndTime), ("Id", log.Id), ("Name", log.Name),
            ("NodeId", log.NodeId), ("StartTime", log.StartTime)),
        Target log => Attributes(log, ("Duration", log.Duration), ("EndTime", log.EndTime), ("Id", log.Id), ("Name", log.Name), ("NodeId", log.NodeId),
            ("StartTime", log.StartTime), ("DependsOnTargets", log.DependsOnTargets), ("SourceFilePath", log.SourceFilePath),
            ("Succeeded", log.Succeeded), ("TargetBuiltReason", log.TargetBuiltReason)),
        Task log => Attributes(log, ("Duration", log.Duration), ("EndTime", log.EndTime), ("Id", log.Id), ("Name", log.Name), ("NodeId", log.NodeId),
            ("StartTime", log.StartTime), ("SourceFilePath", log.SourceFilePath), ("FromAssembly", log.FromAssembly), ("Title", log.Title)),
        Project log => Attributes(log, ("Duration", log.Duration), ("EndTime", log.EndTime), ("Id", log.Id), ("Name", log.Name), ("NodeId", log.NodeId),
            ("StartTime", log.StartTime), ("SourceFilePath", log.SourceFilePath)),
        EntryTarget log => Attributes(log, ("Name", log.Name), ("Target", log.Target)),
        TimedNode log => Attributes(log, ("Duration", log.Duration), ("EndTime", log.EndTime), ("Id", log.Id), ("Name", log.Name), ("NodeId", log.NodeId),
            ("StartTime", log.StartTime)),
        Message log => Attributes(log, ("LineNumber", log.LineNumber), ("TypeName", log.TypeName), ("ShortenedText", log.ShortenedText),
            ("SourceFilePath", log.SourceFilePath), ("Text", log.Text), ("Timestamp", log.Timestamp)),
        Parameter log => Attributes(log, ("Name", log.Name)),
        AddItem log => Attributes(log, ("Name", log.Name)),
        RemoveItem log => Attributes(log, ("Name", log.Name)),
        Warning log => Attributes(log, ("TypeName", log.TypeName), ("Code", log.Code), ("ColumnNumber", log.ColumnNumber), ("EndColumnNumber", log.EndColumnNumber),
            ("EndLineNumber", log.EndLineNumber), ("File", log.File), ("LineNumber", log.LineNumber), ("ProjectFile", log.ProjectFile),
            ("ShortenedText", log.ShortenedText), ("Subcategory", log.Subcategory), ("Text", log.Text), ("Timestamp", log.Timestamp)),
        Error log => Attributes(log, ("TypeName", log.TypeName), ("Code", log.Code), ("ColumnNumber", log.ColumnNumber), ("EndColumnNumber", log.EndColumnNumber),
            ("EndLineNumber", log.EndLineNumber), ("File", log.File), ("LineNumber", log.LineNumber), ("ProjectFile", log.ProjectFile),
            ("ShortenedText", log.ShortenedText), ("Subcategory", log.Subcategory), ("Text", log.Text), ("Timestamp", log.Timestamp)),
        Metadata log => Attributes(log, ("Name", log.Name), ("ShortenedValue", log.ShortenedValue), ("Value", log.Value)),
        BaseNode log => Attributes(log, ("type", log.GetType())),
        _ => null,
    };

    private static IEnumerable<(XName, string?)> Attributes(object _, params (string name, object? value)[] values) =>
        values.Select(i => (XName.Get(i.name, ""), Format(i.value)));

    private static string? Format(object? value) =>
        value is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : value?.ToString();

    private static IEnumerable<(XName, object)>? GetChildren(object item)
    {
        static IEnumerable<(XName, object)> Of(TreeNode node) => node.Children.Select(i => (XName.Get(i.TypeName, ""), (object)i));

        return item switch
        {
            Project log => Of(log).Concat(log.EntryTargets.Select(i => (XName.Get("EntryTarget", ""), (object)i))),
            Item or ProjectEvaluation or Target or Task or Build or Folder or TimedNode or Message or EntryTarget or Parameter or AddItem or RemoveItem or Warning or Error
                => Of((TreeNode)item),
            Metadata or Property or BaseNode or string => null,
            _ => throw new NotSupportedException(item.GetType().ToString()),
        };
    }
}
