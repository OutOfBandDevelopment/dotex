using OoBDev.System.Xml.Linq;
using System.Data.SqlTypes;

namespace OoBDev.System.Data.SqlTypes;

/// <summary>
/// Provides extension methods for converting between SqlXml and XFragment types.
/// </summary>
public static class SqlXmlEx
{
    /// <summary>
    /// Converts a SqlXml instance to an XFragment.
    /// </summary>
    /// <param name="sqlxml">The SqlXml to convert.</param>
    /// <returns>An XFragment representation of the SqlXml content.</returns>
    public static XFragment ToXFragment(this SqlXml sqlxml)
    {
        using var xmlReader = sqlxml.CreateReader();
        return XFragment.Parse(xmlReader);
    }

    /// <summary>
    /// Converts an XFragment to a SqlXml instance.
    /// </summary>
    /// <param name="xFragment">The XFragment to convert.</param>
    /// <returns>A SqlXml representation of the XFragment content.</returns>
    public static SqlXml ToSqlXml(this XFragment xFragment) => new(xFragment.CreateReader());
}
