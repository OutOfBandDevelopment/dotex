namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// A model file that is downloaded into the model folder when it is missing.
/// </summary>
public sealed class ModelFileSource
{
    /// <summary>File name inside the model folder.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Address the file is downloaded from (pin a revision so the content cannot change).</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Expected SHA-256 of the file (hex). A download that does not match is discarded.</summary>
    public string Sha256 { get; set; } = string.Empty;
}
