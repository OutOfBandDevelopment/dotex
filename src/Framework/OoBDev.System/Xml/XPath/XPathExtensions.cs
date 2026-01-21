using OoBDev.System.PathSegments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Provides extension methods for working with XPath navigators, path segments, and node iteration.
/// </summary>
public static class XPathExtensions
{
    /// <summary>
    /// Converts a path segment structure to an XPath expression string.
    /// </summary>
    /// <param name="path">The path segment to convert.</param>
    /// <returns>The XPath expression string.</returns>
    public static string ToXPathExpression(this IPathSegment path) =>
        new XPathExpressionBuilder().BuildXPathExpression(path);

    /// <summary>
    /// Merges multiple XPath navigators into a single composite navigator, combining nodes from all sources.
    /// </summary>
    /// <param name="navigators">The collection of source name and navigator pairs to merge.</param>
    /// <returns>A merged navigator containing all nodes from the input navigators.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the merged result is null.</exception>
    public static IXPathNavigable MergeNavigators(this IEnumerable<(string source, IXPathNavigable? navigator)> navigators) =>
        new WrappedNavigator(WrappedNode.Build(navigators) ?? throw new ArgumentNullException(nameof(navigators)));

    /// <summary>
    /// Merges the specified navigator with additional navigators into a single composite navigator.
    /// </summary>
    /// <param name="navigator">The primary navigator to merge.</param>
    /// <param name="navigators">Additional navigators to merge with the primary navigator.</param>
    /// <returns>A merged navigator containing all nodes from the input navigators.</returns>
    public static IXPathNavigable MergeWith(this (string source, IXPathNavigable? navigator) navigator, params (string source, IXPathNavigable? navigator)[] navigators) =>
         navigator.MergeWith(navigators.AsEnumerable());

    /// <summary>
    /// Merges the specified navigator with a collection of additional navigators into a single composite navigator.
    /// </summary>
    /// <param name="navigator">The primary navigator to merge.</param>
    /// <param name="navigators">Additional navigators to merge with the primary navigator.</param>
    /// <returns>A merged navigator containing all nodes from the input navigators.</returns>
    public static IXPathNavigable MergeWith(this (string source, IXPathNavigable? navigator) navigator, IEnumerable<(string source, IXPathNavigable? navigator)> navigators) =>
        new[] { navigator }.Concat(navigators).MergeNavigators();

    /// <summary>
    /// Converts an XPath node iterator into an enumerable sequence of XPath navigators.
    /// </summary>
    /// <param name="iterator">The XPath node iterator to convert.</param>
    /// <returns>An enumerable sequence of non-null XPath navigators from the iterator.</returns>
    public static IEnumerable<XPathNavigator> AsNavigatorSet(this XPathNodeIterator iterator) =>
        iterator.OfType<IXPathNavigable>()
                .Select(node => node.CreateNavigator())
                .Where(node => node != null)
                .OfType<XPathNavigator>()
                ;

    /// <summary>
    /// Converts an object into a sequence of XPath navigators, handling various enumerable types and converting non-navigable items to text nodes.
    /// Supports IXPathNavigable, IEnumerable&lt;XPathNavigator&gt;, XPathNodeIterator, and other IEnumerable types.
    /// </summary>
    /// <param name="item">The object to convert to a node set.</param>
    /// <returns>An enumerable sequence of XPath navigators representing the input item(s).</returns>
    public static IEnumerable<XPathNavigator?> AsNodeSet(this object item)
    {
        if (item is IEnumerable items)
        {
            var enumerable = items.GetEnumerator();
            while (enumerable.MoveNext())
            {
                var current = enumerable.Current;

                switch (current)
                {
                    case IXPathNavigable nav:
                        yield return nav.CreateNavigator();
                        break;

                    case IEnumerable<XPathNavigator> navs:
                        foreach (var nav in navs)
                        {
                            yield return nav.CreateNavigator();
                        }
                        break;

                    case XPathNodeIterator iterator:
                        while (iterator.MoveNext())
                        {
                            yield return iterator.Current?.CreateNavigator();
                        }
                        break;

                    default:
                        var text = new XText($"{current}");
                        yield return text.ToXPathNavigable().CreateNavigator();
                        break;
                }
            }
        }
        else
        {
            foreach (var child in (new[] { item }).AsNodeSet())
                yield return child;
        }
    }
}
