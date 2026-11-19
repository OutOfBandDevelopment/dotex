using OoBDev.System.Xml.XPath;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using JsonFormatting = Newtonsoft.Json.Formatting;

namespace OoBDev.TestUtilities;

/// <summary>
/// Extension methods for TestContext
/// </summary>
public static class TextContextExtensions
{
    /// <summary>
    /// serialize and store results to test results folder
    /// </summary>
    /// <param name="context">test context</param>
    /// <param name="value">object to store</param>
    /// <param name="fileName"></param>
    /// <returns>test context for chaining</returns>
    public static TestContext AddResult(this TestContext context, object? value, string? fileName)
    {
        if (value == null)
            return context;
        // if (value is IXPathNavigable nav && !(value is XPathNodeIterator)) return AddResult(context, nav.CreateNavigator(), fileName);
        if (value is INode node)
            return AddResult(context, new ExtensibleNavigator(node, fileName), fileName);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            var ext = value switch
            {
                byte[] _ => ".bin",
                Stream _ => ".dat",
                string _ => ".txt",
                XContainer _ => ".xml",
                IXPathNavigable _ => ".xml",
                XPathNodeIterator _ => ".xml",
                _ => ".data"
            };
            fileName = $"{value.GetType().Name}_{TimeProvider.System.GetLocalNow().Ticks}{ext}".Replace('`', '_').Replace(':', '_').Replace('<', '_').Replace('>', '_');
        }

        if (value is byte[] data)
        {
            context.AddResultFile(fileName, data);
        }
        else if (value is Stream stream)
        {
            using var ms = new MemoryStream();
            if (stream.CanSeek)
                stream.Position = 0;
            stream.CopyTo(ms);
            context.AddResultFile(fileName, ms.ToArray());
        }
        else if (value is string content)
        {
            context.AddResultFile(fileName, Encoding.UTF8.GetBytes(content));
        }
        else if (value is XContainer xcontainer)
        {
            context.AddResultFile(fileName, Encoding.UTF8.GetBytes(xcontainer.ToString()));
        }
        else if (value is IXPathNavigable xPathNavigator)
        {
            context.AddResultFile(fileName, Encoding.UTF8.GetBytes(xPathNavigator.CreateNavigator()?.OuterXml ?? ""));
        }
        else if (value is XPathNodeIterator xPathNodeIterator)
        {
            var fragment = xPathNodeIterator.Cast<XPathNavigator>().Aggregate(new StringBuilder(), (sb, x) => sb.AppendLine(x.OuterXml));
            context.AddResultFile(fileName, Encoding.UTF8.GetBytes(fragment.ToString()));
        }
        else if (value != null)
        {
            var json = JsonConvert.SerializeObject(value, JsonFormatting.Indented);
            context.AddResultFile(fileName, Encoding.UTF8.GetBytes(json));
        }
        else
        {
            context.WriteLine("No result");
        }
        return context;
    }
}
