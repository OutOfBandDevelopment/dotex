namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>A class label with its probability.</summary>
/// <param name="Label">Label text.</param>
/// <param name="Probability">Probability from 0 to 1.</param>
public sealed record ImageLabel(string Label, float Probability);
