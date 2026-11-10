using System;
using System.IO;
using AvalonDock;
using AvalonDock.Layout.Serialization;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Saves the main window's dock layout (which panes are where, floating or hidden) as XML and restores it.</summary>
public static class DockLayout
{
    /// <summary>Writes the layout to <paramref name="path"/>; a failure to write is ignored, the layout is a convenience.</summary>
    public static void Save(DockingManager manager, string path)
    {
        ArgumentNullException.ThrowIfNull(manager);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            new XmlLayoutSerializer(manager).Serialize(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
        }
    }

    /// <summary>Restores the layout from <paramref name="path"/>; a missing or unreadable file keeps the default layout.</summary>
    public static bool Load(DockingManager manager, string path)
    {
        ArgumentNullException.ThrowIfNull(manager);
        if (!File.Exists(path))
        {
            return false;
        }

        try
        {
            new XmlLayoutSerializer(manager).Deserialize(path);
            return true;
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or global::System.Xml.XmlException or UnauthorizedAccessException)
        {
            return false;
        }
    }
}
