namespace OoBDev.Imaging.Analysis.Bursts;

/// <summary>Focus of the detected subjects of one frame.</summary>
/// <param name="Count">Number of subjects found; 0 means none.</param>
/// <param name="FocusNorm">Normalised focus of the subjects, if measured.</param>
public sealed record SubjectFocus(int Count, double? FocusNorm);
