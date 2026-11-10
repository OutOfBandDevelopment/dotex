namespace OoBDev.Spike.PluginSandbox;

/// <summary>How the payload bytes are to be read. The transport never interprets the content beyond checking the declared format.</summary>
public enum PayloadFormat : byte
{
    /// <summary>Opaque bytes.</summary>
    Binary = 0,

    /// <summary>A UTF-8 JSON document; the receiver verifies it parses before delivery.</summary>
    Json = 1,

    /// <summary>Strict UTF-8 text.</summary>
    Text = 2,
}
