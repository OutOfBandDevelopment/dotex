namespace OoBDev.DevTerm.DeviceManifests;

/// <summary>A field's place in a frame: where it starts and how many bits it takes (0 for the open-ended last field of a variable frame).</summary>
internal sealed record FrameSlot(FrameField Field, int StartBit, int BitLength)
{
    public int StartByte => StartBit / 8;
}
