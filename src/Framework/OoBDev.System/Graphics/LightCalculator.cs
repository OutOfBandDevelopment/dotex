namespace OoBDev.System.Graphics;

/// <summary>
/// Provides extension methods and static utilities for photographic light calculations, including conversions between lux and exposure value (EV),
/// and calculations for ISO/ASA, f-stop, and shutter speed.
/// </summary>
public static class LightCalculator
{
    /// <summary>
    /// Converts illuminance (lux) to exposure value (EV).
    /// </summary>
    /// <param name="lux">The illuminance in lux.</param>
    /// <returns>The exposure value (EV) rounded to 6 decimal places.</returns>
    public static double LuxToEv(this double lux) =>
        global::System.Math.Round(global::System.Math.Log(lux / 2.5d, 2d), 6);

    /// <summary>
    /// Converts exposure value (EV) to illuminance (lux).
    /// </summary>
    /// <param name="ev">The exposure value (EV).</param>
    /// <returns>The illuminance in lux rounded to 6 decimal places.</returns>
    public static double EvToLux(this double ev) =>
        global::System.Math.Round(global::System.Math.Pow(2, ev) * 2.5, 6);

    /// <summary>
    /// Calculates the illuminance (lux) based on ISO, f-stop, and shutter speed.
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="fStop">The aperture f-stop value.</param>
    /// <param name="shutterSpeed">The shutter speed in seconds.</param>
    /// <returns>The calculated illuminance in lux rounded to 6 decimal places.</returns>
    public static double GetLux(double iso, double fStop, double shutterSpeed) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 250 / (iso * shutterSpeed), 6);

    /// <summary>
    /// Calculates the exposure value (EV) based on ISO, f-stop, and shutter speed.
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="fStop">The aperture f-stop value.</param>
    /// <param name="shutterSpeed">The shutter speed in seconds.</param>
    /// <returns>The calculated exposure value (EV) rounded to 6 decimal places.</returns>
    public static double GetEv(double iso, double fStop, double shutterSpeed) =>
        global::System.Math.Round(global::System.Math.Log(global::System.Math.Pow(fStop, 2) * 100 / (iso * shutterSpeed), 2), 6);

    /// <summary>
    /// Calculates the required ISO/ASA based on f-stop, shutter speed, and illuminance.
    /// </summary>
    /// <param name="fStop">The aperture f-stop value.</param>
    /// <param name="shutterSpeed">The shutter speed in seconds.</param>
    /// <param name="lux">The illuminance in lux.</param>
    /// <returns>The calculated ISO/ASA value rounded to 2 decimal places.</returns>
    public static double GetAsa(double fStop, double shutterSpeed, double lux) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 250 / (lux * shutterSpeed), 2);

    /// <summary>
    /// Calculates the required f-stop based on ISO, shutter speed, and illuminance.
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="shutterSpeed">The shutter speed in seconds.</param>
    /// <param name="lux">The illuminance in lux.</param>
    /// <returns>The calculated f-stop value rounded to 2 decimal places.</returns>
    public static double GetFStop(double iso, double shutterSpeed, double lux) =>
        global::System.Math.Round(global::System.Math.Sqrt(lux * shutterSpeed * iso / 250), 2);

    /// <summary>
    /// Calculates the required shutter speed based on ISO, f-stop, and illuminance.
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="fStop">The aperture f-stop value.</param>
    /// <param name="lux">The illuminance in lux.</param>
    /// <returns>The calculated shutter speed in seconds rounded to 6 decimal places.</returns>
    public static double GetShutterSpeed(double iso, double fStop, double lux) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 250 / (lux * iso), 6);

    /// <summary>
    /// Calculates the required ISO/ASA based on f-stop, shutter speed, and exposure value (EV).
    /// </summary>
    /// <param name="fStop">The aperture f-stop value.</param>
    /// <param name="shutterSpeed">The shutter speed in seconds.</param>
    /// <param name="ev">The exposure value (EV).</param>
    /// <returns>The calculated ISO/ASA value rounded to 2 decimal places.</returns>
    public static double GetAsaE(double fStop, double shutterSpeed, double ev) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 100 / (global::System.Math.Pow(2, ev) * shutterSpeed), 2);

    /// <summary>
    /// Calculates the required f-stop based on ISO, shutter speed, and exposure value (EV).
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="shutterSpeed">The shutter speed in seconds.</param>
    /// <param name="ev">The exposure value (EV).</param>
    /// <returns>The calculated f-stop value rounded to 2 decimal places.</returns>
    public static double GetFStopE(double iso, double shutterSpeed, double ev) =>
        global::System.Math.Round(global::System.Math.Sqrt(global::System.Math.Pow(2, ev) * shutterSpeed * iso / 100), 2);

    /// <summary>
    /// Calculates the required shutter speed based on ISO, f-stop, and exposure value (EV).
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="fStop">The aperture f-stop value.</param>
    /// <param name="ev">The exposure value (EV).</param>
    /// <returns>The calculated shutter speed in seconds rounded to 6 decimal places.</returns>
    public static double GetShutterSpeedE(double iso, double fStop, double ev) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 100 / (global::System.Math.Pow(2, ev) * iso), 6);
}
