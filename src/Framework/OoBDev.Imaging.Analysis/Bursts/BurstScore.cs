namespace OoBDev.Imaging.Analysis.Bursts;

/// <summary>The rank of one frame inside a burst. Advice only: nothing is ever removed or marked.</summary>
/// <param name="Id">The frame id given by the caller.</param>
/// <param name="Score">Higher is better; 0 is the sharpest frame without penalties.</param>
/// <param name="Basis"><c>subject</c> or <c>frame</c>: what focus was compared.</param>
/// <param name="Notes">The reasons, or <c>sharpest</c>.</param>
public sealed record BurstScore(string Id, double Score, string Basis, string Notes);
