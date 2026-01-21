namespace OoBDev.System.Graphics;

public static class LightCalculator
{
    public static double LuxToEv(this double lux) =>
        global::System.Math.Round(global::System.Math.Log(lux / 2.5d, 2d), 6);

    public static double EvToLux(this double ev) =>
        global::System.Math.Round(global::System.Math.Pow(2, ev) * 2.5, 6);

    public static double GetLux(double iso, double fStop, double shutterSpeed) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 250 / (iso * shutterSpeed), 6);

    public static double GetEv(double iso, double fStop, double shutterSpeed) =>
        global::System.Math.Round(global::System.Math.Log(global::System.Math.Pow(fStop, 2) * 100 / (iso * shutterSpeed), 2), 6);

    public static double GetAsa(double fStop, double shutterSpeed, double lux) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 250 / (lux * shutterSpeed), 2);

    public static double GetFStop(double iso, double shutterSpeed, double lux) =>
        global::System.Math.Round(global::System.Math.Sqrt(lux * shutterSpeed * iso / 250), 2);

    public static double GetShutterSpeed(double iso, double fStop, double lux) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 250 / (lux * iso), 6);

    public static double GetAsaE(double fStop, double shutterSpeed, double ev) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 100 / (global::System.Math.Pow(2, ev) * shutterSpeed), 2);

    public static double GetFStopE(double iso, double shutterSpeed, double ev) =>
        global::System.Math.Round(global::System.Math.Sqrt(global::System.Math.Pow(2, ev) * shutterSpeed * iso / 100), 2);

    public static double GetShutterSpeedE(double iso, double fStop, double ev) =>
        global::System.Math.Round(global::System.Math.Pow(fStop, 2) * 100 / (global::System.Math.Pow(2, ev) * iso), 6);
}
