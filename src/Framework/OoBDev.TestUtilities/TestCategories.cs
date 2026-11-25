namespace OoBDev.TestUtilities;

/// <summary>
/// Common test categories
/// </summary>
public static class TestCategories
{
    /// <summary>
    /// Unit tests are rerun-able, standalone tests for a single operation.  External resources should be 
    /// mocked out so these are fast and may run within a pipeline.
    /// </summary>
    public const string Unit = nameof(Unit);

    /// <summary>
    /// Simulation tests are similar to integration tests by testing the majority of the software stack.
    /// The difference being Simulations use mocked entry and persist layers so they may be executed within
    /// a pipeline without requiring external resources. 
    /// </summary>
    public const string Simulate = nameof(Simulate);

    /// <summary>
    /// Integration tests run against Docker-based external services (databases, message queues,
    /// search engines, etc.). Requires Docker containers to be running locally or in CI/CD.
    /// Runs in daily integration test pipeline.
    /// </summary>
    public const string Integration = nameof(Integration);

    /// <summary>
    /// Test points for local development only. Not expected to be safe to rerun and may use
    /// persisted resources. Typically used for manual testing, performance benchmarks, or
    /// exploratory testing. Not executed in CI/CD pipelines.
    /// </summary>
    public const string DevLocal = nameof(DevLocal);

    /// <summary>
    /// Tests that require live cloud services (Groq Cloud, etc.)
    /// which cannot be emulated or containerized locally. Requires valid cloud credentials and
    /// active service subscriptions. Manual execution only, not run in CI/CD pipelines.
    /// </summary>
    public const string LiveIntegration = nameof(LiveIntegration);

    /// <summary>
    /// Tests that need a physical device (serial, USB HID, instruments). They run in the normal test run
    /// and end with <c>Assert.Inconclusive</c> when the device is not detected, so the default run never
    /// depends on hardware.
    /// </summary>
    public const string Hardware = nameof(Hardware);

    // Secondary categories (owner, 2026-11-04, from DevTerm): optional, added beside the one tier above, so
    // dotnet test --filter TestCategory=Serial selects a transport, a feature or a device across every tier.

    // Transports.
    public const string Serial = nameof(Serial);
    public const string Tcp = nameof(Tcp);
    public const string Hid = nameof(Hid);
    public const string Usbtmc = nameof(Usbtmc);
    public const string Ble = nameof(Ble);
    public const string Loopback = nameof(Loopback);
    public const string Rfc2217 = nameof(Rfc2217);
    public const string Vxi11 = nameof(Vxi11);
    public const string Mqtt = nameof(Mqtt);
    public const string Brokers = nameof(Brokers);
    public const string Web = nameof(Web);

    // Device profile types.
    public const string Scpi = nameof(Scpi);

    // Features.

    /// <summary>Session logging and log playback: the format, recorder, engine and each front end.</summary>
    public const string Logging = nameof(Logging);

    /// <summary>
    /// A regression test written from a bug report. Added at the method level beside the test's own tier, so
    /// <c>--filter TestCategory=BugRegression</c> runs every bug regression test.
    /// </summary>
    public const string BugRegression = nameof(BugRegression);

    // Device make and model: one physical unit each, on the tests that exercise that device.
    public const string Tektronix_2230 = nameof(Tektronix_2230);
    public const string Tektronix_Tds2024 = nameof(Tektronix_Tds2024);
    public const string Korad_Ka3005p = nameof(Korad_Ka3005p);
    public const string Korad_Ka6003p = nameof(Korad_Ka6003p);
    public const string Hp_34401a = nameof(Hp_34401a);
    public const string Rigol_Ds1102e = nameof(Rigol_Ds1102e);
    public const string Rigol_Dm3058e = nameof(Rigol_Dm3058e);
    public const string Rigol_Dg1022 = nameof(Rigol_Dg1022);
    public const string Rigol_Dg1062z = nameof(Rigol_Dg1062z);
    public const string Velleman_K8055 = nameof(Velleman_K8055);
    public const string Kuando_Busylight = nameof(Kuando_Busylight);
    public const string Radex_One = nameof(Radex_One);
    public const string Zoom_H4n = nameof(Zoom_H4n);
    public const string DerEe_De5000 = nameof(DerEe_De5000);
    public const string Delorme_EarthmateBt20 = nameof(Delorme_EarthmateBt20);
    public const string Aeotec_ZStickS2 = nameof(Aeotec_ZStickS2);
}
