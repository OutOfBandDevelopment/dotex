namespace OoBDev.Quarta.RadexOne;

/// <summary>
/// The alarm mode byte of the settings.
/// </summary>
public enum RadexOneAlarmMode : byte
{
    /// <summary>No alarm.</summary>
    Off = 0,

    /// <summary>Vibrate.</summary>
    Vibration = 1,

    /// <summary>Sound.</summary>
    Audio = 2,

    /// <summary>Vibrate and sound.</summary>
    VibrationAndAudio = 3,
}
