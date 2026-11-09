namespace OoBDev.DevTerm.Logging;

/// <summary>A playback speed choice: its label and rate (<see cref="double.PositiveInfinity"/> for Max).</summary>
public sealed record PlaybackSpeed(string Label, double Rate)
{
    public override string ToString() => Label;
}
