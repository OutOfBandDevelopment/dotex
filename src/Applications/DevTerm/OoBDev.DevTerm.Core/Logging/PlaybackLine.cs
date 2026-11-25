namespace OoBDev.DevTerm.Logging;

/// <summary>One display line of playback output.</summary>
public sealed record PlaybackLine(string Text, PlaybackLineKind Kind)
{
    public override string ToString() => Text;
}
