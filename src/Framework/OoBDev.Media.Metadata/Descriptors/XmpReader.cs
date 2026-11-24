using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OoBDev.Media.Metadata.Descriptors;

/// <summary>Reads the common descriptive fields (Dublin Core, <c>xmp:</c>, <c>photoshop:</c>) from the XMP packet of a JPEG. Malformed XML gives an empty result.</summary>
public static class XmpReader
{
    private static readonly XNamespace Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private static readonly XNamespace Dc = "http://purl.org/dc/elements/1.1/";
    private static readonly XNamespace Xmp = "http://ns.adobe.com/xap/1.0/";
    private static readonly XNamespace Photoshop = "http://ns.adobe.com/photoshop/1.0/";
    private static readonly byte[] Header = Encoding.ASCII.GetBytes("http://ns.adobe.com/xap/1.0/\0");

    /// <summary>Reads the descriptors of a JPEG file.</summary>
    /// <param name="jpeg">The file bytes.</param>
    /// <returns>The fields, or <see cref="ImageDescriptors.Empty"/>.</returns>
    public static ImageDescriptors Read(byte[] jpeg)
    {
        ArgumentNullException.ThrowIfNull(jpeg);
        if (XmpContainers.Find(jpeg) is { } packet)
            return Parse(packet);
        foreach (var (marker, payload) in JpegSegments.Read(jpeg))
        {
            if (marker == 0xE1 && payload.Length > Header.Length && payload.AsSpan(0, Header.Length).SequenceEqual(Header))
                return Parse(Encoding.UTF8.GetString(payload, Header.Length, payload.Length - Header.Length));
        }

        return ImageDescriptors.Empty;
    }

    /// <summary>Reads the descriptors of an XMP packet.</summary>
    /// <param name="xml">The packet text.</param>
    /// <returns>The fields, or <see cref="ImageDescriptors.Empty"/> when the XML cannot be read.</returns>
    public static ImageDescriptors Parse(string xml)
    {
        ArgumentNullException.ThrowIfNull(xml);
        XDocument doc;
        try
        {
            using var reader = XmlReader.Create(new StringReader(xml.Trim('\0', ' ')), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            doc = XDocument.Load(reader);
        }
        catch (XmlException)
        {
            return ImageDescriptors.Empty;
        }

        var descriptions = doc.Descendants(Rdf + "Description").ToList();
        int? rating = int.TryParse(Values(descriptions, Xmp + "Rating").FirstOrDefault(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var r) ? r : null;
        var found = new ImageDescriptors(
            Values(descriptions, Dc + "title").FirstOrDefault(),
            Values(descriptions, Dc + "creator").FirstOrDefault(),
            Values(descriptions, Dc + "description").FirstOrDefault(),
            Values(descriptions, Dc + "subject"),
            rating,
            Values(descriptions, Photoshop + "DateCreated").Concat(Values(descriptions, Xmp + "CreateDate")).FirstOrDefault());
        return found.IsEmpty ? ImageDescriptors.Empty : found;
    }

    private static List<string> Values(List<XElement> descriptions, XName name)
    {
        var result = new List<string>();
        foreach (var d in descriptions)
        {
            if (d.Attribute(name) is { Value.Length: > 0 } a)
                result.Add(a.Value);
            foreach (var e in d.Elements(name))
            {
                var items = e.Descendants(Rdf + "li").Select(l => l.Value).Where(v => v.Length > 0).ToList();
                if (items.Count > 0)
                    result.AddRange(items);
                else if (e.Value.Length > 0)
                    result.Add(e.Value);
            }
        }

        return result;
    }
}
