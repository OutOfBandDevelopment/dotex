using System;

namespace OoBDev.Kuando.Busylight;

/// <summary>
/// One 8-byte step record: the whole light state, since the light is told its state rather than a change.
/// </summary>
/// <param name="NextStep">The step to run next (0 for a single command).</param>
/// <param name="Repeat">How often to repeat the step.</param>
/// <param name="Color">The colour.</param>
/// <param name="On">On time (unit unconfirmed).</param>
/// <param name="Off">Off time (unit unconfirmed); 0 for a solid light.</param>
/// <param name="Audio">The audio byte fields.</param>
public readonly record struct BusylightCommand(
    byte NextStep,
    byte Repeat,
    BusylightColor Color,
    byte On,
    byte Off,
    BusylightAudio Audio)
{
    /// <summary>The length of one step record.</summary>
    public const int Length = 8;

    /// <summary>A solid colour with the verified defaults.</summary>
    /// <param name="color">The colour.</param>
    /// <returns>The command.</returns>
    public static BusylightCommand Solid(BusylightColor color) => new(0, 0, color, 0x01, 0x00, BusylightAudio.Default);

    /// <summary>Writes the step record: <c>NextStep, Repeat, R, G, B, On, Off, Audio</c>.</summary>
    /// <param name="destination">At least <see cref="Length"/> bytes.</param>
    public void WriteTo(Span<byte> destination)
    {
        destination[0] = NextStep;
        destination[1] = Repeat;
        destination[2] = Color.Red;
        destination[3] = Color.Green;
        destination[4] = Color.Blue;
        destination[5] = On;
        destination[6] = Off;
        destination[7] = Audio.ToByte();
    }
}
