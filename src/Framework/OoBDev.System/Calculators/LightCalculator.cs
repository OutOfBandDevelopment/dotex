using static System.Math;

namespace OoBDev.System.Calculators;

/// <summary>
/// Provides photographic light calculation utilities for computing exposure values based on ISO, aperture, shutter speed, and lux.
/// </summary>
public class LightCalculator
{
    /// <summary>
    /// Calculates the illuminance (lux) based on aperture, ISO, and shutter speed.
    /// </summary>
    /// <param name="aperture">The aperture f-stop value.</param>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="shutter">The shutter speed in seconds.</param>
    /// <returns>The calculated illuminance in lux.</returns>
    public double Lux(double aperture, double iso, double shutter) => Pow(aperture, 2d) * 250d / (iso * shutter);

    /// <summary>
    /// Calculates the required ISO sensitivity based on aperture, illuminance, and shutter speed.
    /// </summary>
    /// <param name="aperture">The aperture f-stop value.</param>
    /// <param name="lux">The illuminance in lux.</param>
    /// <param name="shutter">The shutter speed in seconds.</param>
    /// <returns>The calculated ISO sensitivity value.</returns>
    public double Iso(double aperture, double lux, double shutter) => Pow(aperture, 2d) * 250d / (lux * shutter);

    /// <summary>
    /// Calculates the required aperture f-stop based on ISO, illuminance, and shutter speed.
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="lux">The illuminance in lux.</param>
    /// <param name="shutter">The shutter speed in seconds.</param>
    /// <returns>The calculated aperture f-stop value.</returns>
    public double Aperture(double iso, double lux, double shutter) => Sqrt(lux * shutter / 2.5d * iso / 100);

    /// <summary>
    /// Calculates the required shutter speed based on ISO, illuminance, and aperture.
    /// </summary>
    /// <param name="iso">The ISO sensitivity value.</param>
    /// <param name="lux">The illuminance in lux.</param>
    /// <param name="aperture">The aperture f-stop value.</param>
    /// <returns>The calculated shutter speed in seconds.</returns>
    public double Shutter(double iso, double lux, double aperture) => Pow(aperture, 2d) * 2.5 / lux * 100 / iso;

    /// <summary>
    /// Calculates the exposure value (EV) from illuminance.
    /// </summary>
    /// <param name="lux">The illuminance in lux.</param>
    /// <returns>The calculated exposure value (EV).</returns>
    public double Ev(double lux) => Log(lux / 2.5d, 2d);

    /// <summary>
    /// Calculates the APEX additive system value based on aperture and shutter speed.
    /// </summary>
    /// <param name="aperture">The aperture f-stop value.</param>
    /// <param name="shutter">The shutter speed in seconds.</param>
    /// <returns>The calculated APEX additive system value.</returns>
    public double A(double aperture, double shutter) => Log(Pow(aperture, 2d) / shutter, 2d);
}
