namespace OoBDev.Spike.PluginSandbox;

/// <summary>The outcome of <see cref="PackageVerifier.Verify"/>.</summary>
/// <param name="Refusal">None when the package is accepted.</param>
/// <param name="Detail">The file or field that caused a refusal.</param>
/// <param name="Manifest">The verified manifest; null unless accepted.</param>
public sealed record PackageVerification(PackageRefusal Refusal, string Detail, PluginManifest? Manifest)
{
    /// <summary>True when the package may be installed.</summary>
    public bool Accepted => Refusal == PackageRefusal.None;
}
